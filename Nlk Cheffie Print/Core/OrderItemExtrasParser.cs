using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Nlk_Cheffie_Print.Core.Net;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Core
{
    /// <summary>
    /// Parses order-item add-ons, extras, and variant/options from API / WebSocket JSON.
    /// Supports nlkmenu fields: extras_data, variant_data, plus legacy extras/customizations shapes.
    /// When the API omits extras_data, infers add-ons from unit_price vs product base price.
    /// </summary>
    public sealed class OrderItemParseContext
    {
        public double OrderSubtotal { get; init; }
        public int ItemCount { get; init; }
        public int ItemIndex { get; init; }
    }

    public static class OrderItemExtrasParser
    {
        public static List<string> ParseAddedExtras(JsonElement item, OrderItemParseContext? context = null)
        {
            var added = new List<string>();
            ParseExtrasArray(item, "extras_data", added);
            ParseExtrasArray(item, "extras", added);
            ParseExtrasArray(item, "additions", added);
            ParseExtrasArray(item, "modifiers", added);
            ParseExtrasArray(item, "options", added);
            ParseCustomizationsAdded(item, added);
            ParseVariantData(item, added);

            if (added.Count == 0 && context != null)
                InferMissingExtras(item, added, context);
            else if (added.Count > 0 && added.All(IsGenericExtraLabel) && context != null)
            {
                added.Clear();
                InferMissingExtras(item, added, context);
            }

            return added;
        }

        public static void EnrichOrderFromJson(Order order, JsonElement root)
        {
            JsonElement itemsEl = default;
            if (root.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                itemsEl = apiItems;
            else if (root.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                itemsEl = wsItems;

            if (itemsEl.ValueKind != JsonValueKind.Array || order.Items.Count == 0)
                return;

            var jsonItems = itemsEl.EnumerateArray().ToList();
            double orderSubtotal = TryParseDouble(order.Subtotal);

            for (int i = 0; i < order.Items.Count && i < jsonItems.Count; i++)
            {
                var jsonItem = jsonItems[i];
                var orderItem = order.Items[i];
                var context = new OrderItemParseContext
                {
                    OrderSubtotal = orderSubtotal,
                    ItemCount = order.Items.Count,
                    ItemIndex = i
                };

                if (!HasStructuredExtras(jsonItem))
                {
                    orderItem.AddedCustomizations.Clear();
                    InferMissingExtras(jsonItem, orderItem.AddedCustomizations, context);
                }
                else if (orderItem.AddedCustomizations.Count == 0)
                {
                    InferMissingExtras(jsonItem, orderItem.AddedCustomizations, context);
                }

                double paidLineTotal = GetPaidLineTotal(jsonItem, orderItem.Quantity);
                if (paidLineTotal <= 0 && order.Items.Count == 1 && orderSubtotal > 0)
                    paidLineTotal = orderSubtotal;

                double baseUnit = GetBaseUnitPrice(jsonItem);
                if (baseUnit > 0)
                    orderItem.UnitPrice = baseUnit.ToString("0.00", CultureInfo.InvariantCulture);

                if (paidLineTotal > 0)
                    orderItem.LineTotal = paidLineTotal.ToString("0.00", CultureInfo.InvariantCulture);
            }

            if (TryParseDouble(order.ExtrasTotal) <= 0.009)
            {
                double fromLabels = SumExtrasFromOrderItemLabels(order.Items);
                if (fromLabels > 0.009)
                {
                    order.ExtrasTotal = fromLabels.ToString("0.00", CultureInfo.InvariantCulture);
                }
                else
                {
                    double calculated = CalculateExtrasTotalFromItems(jsonItems, orderSubtotal);
                    if (calculated > 0.009)
                        order.ExtrasTotal = calculated.ToString("0.00", CultureInfo.InvariantCulture);
                }
            }
            else if (TryParseDouble(order.ExtrasTotal) >= orderSubtotal - 0.009 && orderSubtotal > 0)
            {
                double fromLabels = SumExtrasFromOrderItemLabels(order.Items);
                if (fromLabels > 0.009)
                    order.ExtrasTotal = fromLabels.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        public static double SumExtrasFromOrderItemLabels(IEnumerable<OrderItem> items)
        {
            double total = 0;
            foreach (var item in items)
                total += SumPricesFromExtraLabels(item.AddedCustomizations) * Math.Max(1, item.Quantity);
            return total;
        }

        public static double SumPricesFromExtraLabels(IEnumerable<string> labels)
        {
            double total = 0;
            foreach (var label in labels)
            {
                foreach (Match match in ExtraPricePattern.Matches(label))
                {
                    string raw = match.Groups[1].Value.Replace(',', '.');
                    if (TryParseDouble(raw, out double price) && price > 0)
                        total += price;
                }
            }
            return total;
        }

        private static readonly Regex ExtraPricePattern = new(@"\(\+([\d.,]+)\s*TL\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static double CalculateExtrasTotalFromItems(IReadOnlyList<JsonElement> jsonItems, double orderSubtotal)
        {
            double fromLabels = 0;
            for (int i = 0; i < jsonItems.Count; i++)
            {
                var context = new OrderItemParseContext
                {
                    OrderSubtotal = orderSubtotal,
                    ItemCount = jsonItems.Count,
                    ItemIndex = i
                };
                var added = ParseAddedExtras(jsonItems[i], context);
                fromLabels += SumPricesFromExtraLabels(added) * GetQuantity(jsonItems[i]);
            }
            if (fromLabels > 0.009)
                return fromLabels;

            double baseTotal = 0;
            foreach (var item in jsonItems)
                baseTotal += GetBaseUnitPrice(item) * GetQuantity(item);

            if (orderSubtotal > 0 && baseTotal > 0.009 && orderSubtotal > baseTotal + 0.009)
                return orderSubtotal - baseTotal;

            double inferredItemExtras = 0;
            for (int i = 0; i < jsonItems.Count; i++)
            {
                double paid = GetPaidLineTotal(jsonItems[i], GetQuantity(jsonItems[i]));
                if (paid <= 0 && jsonItems.Count == 1 && orderSubtotal > 0)
                    paid = orderSubtotal;

                double baseLine = GetBaseUnitPrice(jsonItems[i]) * GetQuantity(jsonItems[i]);
                if (baseLine > 0.009 && paid > baseLine + 0.009)
                    inferredItemExtras += paid - baseLine;
            }

            return inferredItemExtras;
        }

        public static double ResolveExtrasTotal(JsonElement slip, IReadOnlyList<JsonElement>? jsonItems = null)
        {
            jsonItems ??= GetItemsArray(slip);
            double subtotal = TryParseDouble(GetJsonStr(slip, "subtotal"));
            if (subtotal <= 0 && slip.TryGetProperty("payment_info", out var payInfo) && payInfo.ValueKind == JsonValueKind.Object)
                subtotal = TryParseDouble(GetJsonStr(payInfo, "subtotal"));

            double fromField = 0;
            if (slip.TryGetProperty("payment_info", out var pay) && pay.ValueKind == JsonValueKind.Object)
                fromField = TryParseDouble(GetJsonStr(pay, "extras_total"));
            if (fromField <= 0)
                fromField = TryParseDouble(GetJsonStr(slip, "extras_total"));

            if (fromField > 0.009 && (subtotal <= 0 || fromField < subtotal - 0.009))
                return fromField;

            return CalculateExtrasTotalFromItems(jsonItems, subtotal);
        }

        public static List<JsonElement> GetItemsArray(JsonElement slip)
        {
            if (slip.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                return apiItems.EnumerateArray().ToList();
            if (slip.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                return wsItems.EnumerateArray().ToList();
            return new List<JsonElement>();
        }

        public static void InferMissingExtras(JsonElement item, List<string> added, OrderItemParseContext context)
        {
            int qty = GetQuantity(item);
            double baseUnit = GetBaseUnitPrice(item);
            if (baseUnit <= 0 || qty <= 0)
                return;

            double paidLineTotal = GetPaidLineTotal(item, qty);
            if (paidLineTotal <= 0 && context.ItemCount == 1 && context.OrderSubtotal > 0)
                paidLineTotal = context.OrderSubtotal;

            double diff = paidLineTotal - (baseUnit * qty);
            if (diff <= 0.009)
                return;

            double unitExtra = diff / qty;
            foreach (var entry in ResolveInferredExtraEntries(item, unitExtra, GetProductId(item), GetProductName(item)))
                AddUnique(added, $"{entry.Name} (+{entry.Price:0.00} TL)");
        }

        private static bool HasStructuredExtras(JsonElement item)
        {
            if (!item.TryGetProperty("extras_data", out var extrasData))
                return false;

            return extrasData.ValueKind switch
            {
                JsonValueKind.Array => extrasData.GetArrayLength() > 0,
                JsonValueKind.Object => true,
                JsonValueKind.String => !string.IsNullOrWhiteSpace(extrasData.GetString()),
                _ => false
            };
        }

        private static bool IsGenericExtraLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return false;

            string genericTr = LocalizationService.T("receipt.extra", "Ekstra");
            return label.StartsWith(genericTr + " (+", StringComparison.OrdinalIgnoreCase)
                || label.StartsWith("Ekstra (+", StringComparison.OrdinalIgnoreCase)
                || label.StartsWith("Extra (+", StringComparison.OrdinalIgnoreCase);
        }

        private static List<(string Name, double Price)> ResolveInferredExtraEntries(JsonElement item, double unitExtraAmount, int fallbackProductId = 0, string fallbackProductName = "")
        {
            int productId = GetProductId(item);
            if (productId <= 0)
                productId = fallbackProductId;

            if (productId > 0)
            {
                var fromCatalog = ProductExtrasCatalog.ResolveExtras(productId, unitExtraAmount);
                if (fromCatalog.Count > 0)
                    return fromCatalog;
            }

            if (!string.IsNullOrWhiteSpace(fallbackProductName))
            {
                var byName = ProductExtrasCatalog.ResolveExtrasByProductName(fallbackProductName, unitExtraAmount);
                if (byName.Count > 0)
                    return byName;
            }

            string? fromProduct = TryMatchExtraNameFromProduct(item, unitExtraAmount);
            if (!string.IsNullOrWhiteSpace(fromProduct))
                return new List<(string, double)> { (fromProduct, unitExtraAmount) };

            return new List<(string, double)> { (LocalizationService.T("receipt.extra", "Ekstra"), unitExtraAmount) };
        }

        public static void RefreshOrderExtraNames(Order order)
        {
            if (!ProductExtrasCatalog.IsLoaded || order.Items.Count == 0)
                return;

            foreach (var orderItem in order.Items)
            {
                double unitExtra = ExtractUnitExtraPrice(orderItem);
                if (unitExtra <= 0.009)
                    continue;

                var resolved = ProductExtrasCatalog.ResolveExtras(orderItem.ProductId, unitExtra);
                if (resolved.Count == 0)
                    resolved = ProductExtrasCatalog.ResolveExtrasByProductName(orderItem.Name, unitExtra);

                if (resolved.Count == 0)
                    continue;

                orderItem.AddedCustomizations = resolved
                    .Select(r => $"{r.Name} (+{r.Price:0.00} TL)")
                    .ToList();
            }

            double extrasTotal = SumExtrasFromOrderItemLabels(order.Items);
            if (extrasTotal > 0.009)
                order.ExtrasTotal = extrasTotal.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static double ExtractUnitExtraPrice(OrderItem item)
        {
            double fromLabels = SumPricesFromExtraLabels(item.AddedCustomizations);
            if (fromLabels > 0.009)
                return fromLabels;

            if (TryParseDouble(item.UnitPrice, out double baseUnit) &&
                TryParseDouble(item.LineTotal, out double lineTotal) &&
                item.Quantity > 0)
            {
                double diff = lineTotal - (baseUnit * item.Quantity);
                if (diff > 0.009)
                    return diff / item.Quantity;
            }

            return 0;
        }

        public static List<string> ParseRemovedExtras(JsonElement item)
        {
            var removed = new List<string>();
            if (!item.TryGetProperty("customizations", out var custEl)) return removed;

            if (custEl.ValueKind == JsonValueKind.Object &&
                custEl.TryGetProperty("removed", out var removedEl))
            {
                AppendFormattedEntries(removedEl, removed);
            }

            return removed;
        }

        private static void ParseCustomizationsAdded(JsonElement item, List<string> added)
        {
            if (!item.TryGetProperty("customizations", out var custEl)) return;

            if (custEl.ValueKind == JsonValueKind.Array)
            {
                AppendFormattedEntries(custEl, added);
            }
            else if (custEl.ValueKind == JsonValueKind.Object &&
                     custEl.TryGetProperty("added", out var addedEl))
            {
                AppendFormattedEntries(addedEl, added);
            }
        }

        private static void ParseExtrasArray(JsonElement item, string propertyName, List<string> target)
        {
            if (!item.TryGetProperty(propertyName, out var element)) return;
            AppendFormattedEntries(element, target);
        }

        private static void AppendFormattedEntries(JsonElement element, List<string> target)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                string raw = element.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(raw)) return;

                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    AppendFormattedEntries(doc.RootElement, target);
                }
                catch
                {
                    AddUnique(target, raw.Trim());
                }
                return;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in element.EnumerateArray())
                {
                    string formatted = FormatEntry(entry);
                    AddUnique(target, formatted);
                }
                return;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                AddUnique(target, FormatEntry(element));
            }
        }

        private static void ParseVariantData(JsonElement item, List<string> added)
        {
            if (!item.TryGetProperty("variant_data", out var variantEl)) return;

            if (variantEl.ValueKind == JsonValueKind.String)
            {
                string raw = variantEl.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(raw)) return;

                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    ParseVariantObject(doc.RootElement, added);
                }
                catch
                {
                    AddUnique(added, raw.Trim());
                }
                return;
            }

            ParseVariantObject(variantEl, added);
        }

        private static void ParseVariantObject(JsonElement variantEl, List<string> added)
        {
            if (variantEl.ValueKind != JsonValueKind.Object) return;

            if (variantEl.TryGetProperty("option_values", out var optionValues))
            {
                ParseVariantObject(optionValues, added);
                return;
            }

            foreach (var prop in variantEl.EnumerateObject())
            {
                if (prop.Name is "id" or "variant_id" or "product_variant_id") continue;

                string value = GetLocalizedText(prop.Value);
                if (string.IsNullOrWhiteSpace(value)) continue;

                string entry = prop.Name.Contains(' ') || LooksLikeLabel(prop.Name)
                    ? $"{prop.Name}: {value}"
                    : value;

                AddUnique(added, entry);
            }
        }

        private static bool LooksLikeLabel(string name)
        {
            return name.Length > 1 && char.IsUpper(name[0]) && !name.Contains('_');
        }

        private static string FormatEntry(JsonElement entry)
        {
            if (entry.ValueKind == JsonValueKind.String)
                return entry.GetString()?.Trim() ?? "";

            if (entry.ValueKind != JsonValueKind.Object)
                return "";

            string name = GetLocalizedTextFromObject(entry, "name");
            if (string.IsNullOrEmpty(name)) name = GetLocalizedTextFromObject(entry, "option_name");
            if (string.IsNullOrEmpty(name)) name = GetLocalizedTextFromObject(entry, "title");
            if (string.IsNullOrEmpty(name)) name = GetLocalizedTextFromObject(entry, "label");

            string group = GetLocalizedTextFromObject(entry, "group");
            if (string.IsNullOrEmpty(group)) group = GetLocalizedTextFromObject(entry, "group_name");

            string priceText = GetJsonStr(entry, "price");
            if (string.IsNullOrEmpty(priceText)) priceText = GetJsonStr(entry, "amount");
            if (string.IsNullOrEmpty(priceText)) priceText = GetJsonStr(entry, "extra_price");

            if (double.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out double priceVal) && priceVal > 0)
            {
                name = string.IsNullOrEmpty(name)
                    ? $"(+{priceVal:0.00} TL)"
                    : $"{name} (+{priceVal:0.00} TL)";
            }

            if (!string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(name))
                return $"{group}: {name}";

            return name;
        }

        private static string GetLocalizedTextFromObject(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var prop)) return "";
            return GetLocalizedText(prop);
        }

        private static string GetLocalizedText(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString()?.Trim() ?? "",
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Object => ResolveLocalizedObject(element),
                _ => ""
            };
        }

        private static string ResolveLocalizedObject(JsonElement element)
        {
            string language = ConfigManager.Current.App.Language;
            if (element.TryGetProperty(language, out var localized))
                return GetLocalizedText(localized);
            if (element.TryGetProperty("tr", out var turkish))
                return GetLocalizedText(turkish);
            if (element.TryGetProperty("en", out var english))
                return GetLocalizedText(english);
            if (element.TryGetProperty("name", out var name))
                return GetLocalizedText(name);
            return "";
        }

        private static string GetJsonStr(JsonElement el, string prop)
        {
            if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(prop, out var p))
                return "";

            return p.ValueKind switch
            {
                JsonValueKind.String => p.GetString() ?? "",
                JsonValueKind.Number => p.GetDouble().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => ""
            };
        }

        private static void AddUnique(List<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (!target.Contains(value))
                target.Add(value);
        }

        public static int CustomizationScore(JsonElement item)
        {
            int score = 0;
            if (item.TryGetProperty("extras_data", out var ed) && ed.ValueKind != JsonValueKind.Null) score += 4;
            if (item.TryGetProperty("extras", out var ex) && ex.ValueKind == JsonValueKind.Array && ex.GetArrayLength() > 0) score += 3;
            if (item.TryGetProperty("variant_data", out var vd) && vd.ValueKind != JsonValueKind.Null) score += 2;
            if (item.TryGetProperty("customizations", out var c) && c.ValueKind != JsonValueKind.Null) score += 1;
            return score;
        }

        public static int GetProductId(JsonElement item)
        {
            if (item.TryGetProperty("product_id", out var directId) && directId.ValueKind == JsonValueKind.Number)
                return directId.GetInt32();

            if (item.TryGetProperty("product", out var product) && product.ValueKind == JsonValueKind.Object &&
                product.TryGetProperty("id", out var nestedId) && nestedId.ValueKind == JsonValueKind.Number)
                return nestedId.GetInt32();

            return 0;
        }

        private static string GetProductName(JsonElement item)
        {
            string name = GetJsonStr(item, "name");
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            if (item.TryGetProperty("product", out var product) && product.ValueKind == JsonValueKind.Object)
            {
                name = GetLocalizedTextFromObject(product, "name");
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            return GetJsonStr(item, "product_name");
        }

        private static int GetQuantity(JsonElement item)
        {
            if (item.TryGetProperty("quantity", out var q) && q.ValueKind == JsonValueKind.Number)
                return Math.Max(1, q.GetInt32());
            return 1;
        }

        private static double GetBaseUnitPrice(JsonElement item)
        {
            double basePrice = TryParseDouble(GetJsonStr(item, "base_price"));
            if (basePrice > 0)
                return basePrice;

            if (item.TryGetProperty("product", out var product) && product.ValueKind == JsonValueKind.Object)
            {
                basePrice = TryParseDouble(GetJsonStr(product, "price"));
                if (basePrice > 0)
                    return basePrice;
            }

            return 0;
        }

        private static double GetPaidLineTotal(JsonElement item, int qty)
        {
            double totalPrice = TryParseDouble(GetJsonStr(item, "total_price"));
            if (totalPrice > 0)
                return totalPrice;

            totalPrice = TryParseDouble(GetJsonStr(item, "subtotal"));
            if (totalPrice > 0)
                return totalPrice;

            totalPrice = TryParseDouble(GetJsonStr(item, "line_total"));
            if (totalPrice > 0)
                return totalPrice;

            double unitPrice = TryParseDouble(GetJsonStr(item, "unit_price"));
            if (unitPrice <= 0)
                unitPrice = TryParseDouble(GetJsonStr(item, "price"));

            if (unitPrice > 0)
                return unitPrice * qty;

            return 0;
        }

        private static string? TryMatchExtraNameFromProduct(JsonElement item, double unitExtraAmount)
        {
            if (!item.TryGetProperty("product", out var product) || product.ValueKind != JsonValueKind.Object)
                return null;
            if (!product.TryGetProperty("extras", out var extras) || extras.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var extra in extras.EnumerateArray())
            {
                double price = TryParseDouble(GetJsonStr(extra, "price"));
                if (Math.Abs(price - unitExtraAmount) < 0.01)
                {
                    string name = GetLocalizedTextFromObject(extra, "name");
                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }
            }

            return null;
        }

        private static double TryParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                ? result
                : 0;
        }

        private static bool TryParseDouble(string? value, out double result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
    }
}

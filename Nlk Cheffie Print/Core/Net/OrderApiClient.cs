using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Core.Net
{
    public static class OrderApiClient
    {
        public static HttpClient CreateAuthenticatedClient()
        {
            var app = ConfigManager.Current.App;
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "CheffiePOS-PrintBridge/1.0");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", app.DeviceToken);

            if (!string.IsNullOrEmpty(app.RestaurantSlug))
            {
                client.DefaultRequestHeaders.Add("X-Restaurant-Slug", app.RestaurantSlug);
                client.DefaultRequestHeaders.Add("RestaurantSlug", app.RestaurantSlug);
            }

            return client;
        }

        public static async Task<JsonElement?> FetchOrderDetailAsync(HttpClient client, string orderKey, CancellationToken token = default)
        {
            string baseUrl = ConfigManager.Current.App.ApiBaseUrl.TrimEnd('/');
            string encodedKey = Uri.EscapeDataString(orderKey);

            string[] candidates =
            {
                $"{baseUrl}/printer/orders/{encodedKey}",
                $"{baseUrl}/restaurant/orders/{encodedKey}",
                $"{baseUrl}/restaurant/order/{encodedKey}"
            };

            foreach (var url in candidates)
            {
                try
                {
                    var response = await client.GetAsync(url, token);
                    if (!response.IsSuccessStatusCode) continue;

                    string json = await response.Content.ReadAsStringAsync(token);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                    {
                        if (dataProp.TryGetProperty("order", out var orderProp))
                            return orderProp.Clone();
                        return dataProp.Clone();
                    }

                    if (root.TryGetProperty("order", out var directOrderProp))
                        return directOrderProp.Clone();

                    return root.Clone();
                }
                catch
                {
                    // Try the next endpoint candidate.
                }
            }

            return null;
        }

        public static JsonElement MergeOrderDetails(JsonElement baseOrder, JsonElement? detailOrder)
        {
            if (detailOrder == null) return baseOrder;

            var dict = new Dictionary<string, object>();
            foreach (var prop in baseOrder.EnumerateObject())
                dict[prop.Name] = GetValue(prop.Value);

            foreach (var prop in detailOrder.Value.EnumerateObject())
            {
                if (!dict.ContainsKey(prop.Name) || IsEmptyValue(dict[prop.Name]))
                {
                    dict[prop.Name] = GetValue(prop.Value);
                    continue;
                }

                if (prop.Name is "order_items" or "items")
                    dict[prop.Name] = MergeOrderItems(dict[prop.Name], GetValue(prop.Value));
            }

            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(dict));
            return doc.RootElement.Clone();
        }

        private static object MergeOrderItems(object baseItemsObj, object detailItemsObj)
        {
            var detailItems = ToJsonElements(detailItemsObj);
            if (detailItems.Count > 0)
                return detailItemsObj;

            return baseItemsObj;
        }

        private static object MergeOrderItem(JsonElement baseItem, JsonElement detailItem)
        {
            int baseScore = OrderItemExtrasParser.CustomizationScore(baseItem);
            int detailScore = OrderItemExtrasParser.CustomizationScore(detailItem);
            if (detailScore > baseScore) return GetValue(detailItem);
            if (baseScore > detailScore) return GetValue(baseItem);

            var dict = new Dictionary<string, object>();
            foreach (var prop in baseItem.EnumerateObject())
                dict[prop.Name] = GetValue(prop.Value);

            foreach (var prop in detailItem.EnumerateObject())
            {
                if (!dict.ContainsKey(prop.Name) || IsEmptyValue(dict[prop.Name]))
                    dict[prop.Name] = GetValue(prop.Value);
            }

            return dict;
        }

        private static List<JsonElement> ToJsonElements(object itemsObj)
        {
            var result = new List<JsonElement>();
            if (itemsObj is not List<object> list) return result;

            foreach (var entry in list)
            {
                if (entry is JsonElement element)
                    result.Add(element);
                else
                {
                    string json = JsonSerializer.Serialize(entry);
                    using var doc = JsonDocument.Parse(json);
                    result.Add(doc.RootElement.Clone());
                }
            }

            return result;
        }

        private static object GetValue(JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => el.GetDouble(),
                JsonValueKind.String => el.GetString() ?? "",
                JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(el.GetRawText()) ?? new List<object>(),
                JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(el.GetRawText()) ?? new Dictionary<string, object>(),
                _ => null!
            };
        }

        private static bool IsEmptyValue(object val)
        {
            if (val == null) return true;
            if (val is string s && string.IsNullOrEmpty(s)) return true;
            if (val is List<object> l && l.Count == 0) return true;
            if (val is Dictionary<string, object> d && d.Count == 0) return true;
            return false;
        }
    }
}

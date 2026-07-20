using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nlk_Cheffie_Print.Core.Net
{
    internal sealed class CatalogExtra
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public double Price { get; init; }
    }

    /// <summary>
    /// Loads product extras from the public menu API and resolves extra names by product + price.
    /// </summary>
    public static class ProductExtrasCatalog
    {
        private static readonly object Sync = new();
        private static Dictionary<int, List<CatalogExtra>> _byProduct = new();
        private static Dictionary<string, int> _productIdsByName = new(StringComparer.OrdinalIgnoreCase);
        private static DateTime _loadedAt = DateTime.MinValue;
        private static Task? _loadTask;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

        public static bool IsLoaded
        {
            get
            {
                lock (Sync)
                {
                    return _byProduct.Count > 0 && DateTime.UtcNow - _loadedAt < CacheTtl;
                }
            }
        }

        public static async Task EnsureLoadedAsync(CancellationToken token = default)
        {
            if (IsLoaded) return;

            Task task;
            lock (Sync)
            {
                if (IsLoaded) return;
                _loadTask ??= LoadInternalAsync(token);
                task = _loadTask;
            }

            await task.ConfigureAwait(false);
        }

        public static List<(string Name, double Price)> ResolveExtras(int productId, double unitExtraAmount)
        {
            if (productId <= 0 || unitExtraAmount <= 0.009)
                return new List<(string, double)>();

            List<CatalogExtra> extras;
            lock (Sync)
            {
                if (!_byProduct.TryGetValue(productId, out extras!) || extras.Count == 0)
                    return new List<(string, double)>();
                extras = extras.ToList();
            }

            var single = extras.Where(e => Math.Abs(e.Price - unitExtraAmount) < 0.01).ToList();
            if (single.Count == 1)
                return new List<(string, double)> { (single[0].Name, single[0].Price) };

            var combo = FindPriceCombination(extras, unitExtraAmount);
            if (combo.Count > 0)
                return combo.Select(e => (e.Name, e.Price)).ToList();

            if (single.Count > 0)
                return single.Select(e => (e.Name, e.Price)).ToList();

            return new List<(string, double)>();
        }

        public static List<(string Name, double Price)> ResolveExtrasByProductName(string productName, double unitExtraAmount)
        {
            if (string.IsNullOrWhiteSpace(productName) || unitExtraAmount <= 0.009)
                return new List<(string, double)>();

            int productId;
            lock (Sync)
            {
                _productIdsByName.TryGetValue(productName.Trim(), out productId);
            }

            return productId > 0 ? ResolveExtras(productId, unitExtraAmount) : new List<(string, double)>();
        }

        private static async Task LoadInternalAsync(CancellationToken token)
        {
            try
            {
                var app = ConfigManager.Current.App;
                string slug = app.RestaurantSlug;
                string restaurantId = app.RestaurantId;
                if (string.IsNullOrWhiteSpace(slug) && string.IsNullOrWhiteSpace(restaurantId))
                    return;

                string baseUrl = app.ApiBaseUrl.TrimEnd('/');
                string lang = app.Language;
                string url = string.IsNullOrWhiteSpace(slug)
                    ? $"{baseUrl}/public/menu?restaurant_id={Uri.EscapeDataString(restaurantId)}&lang={Uri.EscapeDataString(lang)}"
                    : $"{baseUrl}/public/menu?slug={Uri.EscapeDataString(slug)}&lang={Uri.EscapeDataString(lang)}";

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "CheffiePOS-PrintBridge/1.0");

                var response = await client.GetAsync(url, token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return;

                string json = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                var parsed = ParseMenu(json);

                lock (Sync)
                {
                    _byProduct = parsed;
                    _loadedAt = DateTime.UtcNow;
                }
            }
            catch
            {
                // Keep stale cache if reload fails.
            }
            finally
            {
                lock (Sync)
                {
                    _loadTask = null;
                }
            }
        }

        private static Dictionary<int, List<CatalogExtra>> ParseMenu(string json)
        {
            var result = new Dictionary<int, List<CatalogExtra>>();
            var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            JsonElement categories = default;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object &&
                data.TryGetProperty("categories", out var dataCategories))
            {
                categories = dataCategories;
            }
            else if (root.TryGetProperty("categories", out var rootCategories))
            {
                categories = rootCategories;
            }

            if (categories.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var category in categories.EnumerateArray())
            {
                if (!category.TryGetProperty("products", out var products) || products.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var product in products.EnumerateArray())
                {
                    if (!product.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.Number)
                        continue;

                    int productId = idProp.GetInt32();
                    string productName = ReadLocalizedName(product, "name");
                    if (!string.IsNullOrWhiteSpace(productName))
                        names[productName.Trim()] = productId;

                    if (!product.TryGetProperty("extras", out var extrasEl) || extrasEl.ValueKind != JsonValueKind.Array)
                        continue;

                    var list = new List<CatalogExtra>();
                    foreach (var extra in extrasEl.EnumerateArray())
                    {
                        string name = ReadLocalizedName(extra, "name");
                        double price = ReadDouble(extra, "price");
                        if (string.IsNullOrWhiteSpace(name) || price <= 0)
                            continue;

                        int id = extra.TryGetProperty("id", out var extraId) && extraId.ValueKind == JsonValueKind.Number
                            ? extraId.GetInt32()
                            : 0;

                        list.Add(new CatalogExtra { Id = id, Name = name, Price = price });
                    }

                    if (list.Count > 0)
                        result[productId] = list;
                }
            }

            lock (Sync)
            {
                _productIdsByName = names;
            }

            return result;
        }

        private static List<CatalogExtra> FindPriceCombination(List<CatalogExtra> extras, double target)
        {
            var result = new List<CatalogExtra>();
            if (TryBuildCombination(extras, target, 0, new List<CatalogExtra>(), result))
                return result;
            return new List<CatalogExtra>();
        }

        private static bool TryBuildCombination(
            List<CatalogExtra> extras,
            double target,
            int startIndex,
            List<CatalogExtra> current,
            List<CatalogExtra> result)
        {
            if (Math.Abs(SumPrices(current) - target) < 0.01 && current.Count > 0)
            {
                result.Clear();
                result.AddRange(current);
                return true;
            }

            if (SumPrices(current) > target + 0.01 || startIndex >= extras.Count)
                return false;

            for (int i = startIndex; i < extras.Count; i++)
            {
                current.Add(extras[i]);
                if (TryBuildCombination(extras, target, i + 1, current, result))
                    return true;
                current.RemoveAt(current.Count - 1);
            }

            return false;
        }

        private static double SumPrices(IEnumerable<CatalogExtra> extras)
            => extras.Sum(e => e.Price);

        private static string ReadLocalizedName(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var prop))
                return "";

            if (prop.ValueKind == JsonValueKind.String)
                return prop.GetString()?.Trim() ?? "";

            if (prop.ValueKind == JsonValueKind.Object)
            {
                string lang = ConfigManager.Current.App.Language;
                if (prop.TryGetProperty(lang, out var localized) && localized.ValueKind == JsonValueKind.String)
                    return localized.GetString()?.Trim() ?? "";
                if (prop.TryGetProperty("tr", out var tr) && tr.ValueKind == JsonValueKind.String)
                    return tr.GetString()?.Trim() ?? "";
                if (prop.TryGetProperty("en", out var en) && en.ValueKind == JsonValueKind.String)
                    return en.GetString()?.Trim() ?? "";
            }

            return "";
        }

        private static double ReadDouble(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var prop))
                return 0;

            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetDouble(),
                JsonValueKind.String => double.TryParse(prop.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ? val : 0,
                _ => 0
            };
        }
    }
}

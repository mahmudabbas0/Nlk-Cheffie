using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Core.Net
{
    public class HttpOrderPoller
    {
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private HashSet<string> _seenOrderIds = new HashSet<string>();

        public event Action<JsonElement, string>? OrderReceived;

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _isRunning = true;
            _seenOrderIds.Clear();

            Task.Run(() => RunApiPollLoop(_cts.Token));
        }

        public void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
        }

        private async Task RunApiPollLoop(CancellationToken token)
        {
            int backoff = 5;
            string lastToken = "";

            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    var app = ConfigManager.Current.App;
                    string deviceToken = app.DeviceToken;
                    string baseUrl = app.ApiBaseUrl.TrimEnd('/');
                    string slug = app.RestaurantSlug;

                    if (!ConfigManager.IsSecureApiUrl(baseUrl))
                    {
                        LogMessage("[API POLL ERROR] Güvenli olmayan API adresi reddedildi.");
                        await Task.Delay(30000, token);
                        continue;
                    }

                    if (string.IsNullOrEmpty(deviceToken))
                    {
                        await Task.Delay(5000, token);
                        continue;
                    }

                    if (deviceToken != lastToken)
                    {
                        lastToken = deviceToken;
                        _seenOrderIds.Clear();
                    }

                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(15);
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Add("User-Agent", "CheffiePOS-PrintBridge/1.0");
                        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deviceToken);

                        if (!string.IsNullOrEmpty(slug))
                        {
                            client.DefaultRequestHeaders.Add("X-Restaurant-Slug", slug);
                            client.DefaultRequestHeaders.Add("RestaurantSlug", slug);
                        }

                        string url = $"{baseUrl}/printer/orders?per_page=50";
                        var response = await client.GetAsync(url, token);
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync(token);
                            using (var doc = JsonDocument.Parse(json))
                            {
                                var root = doc.RootElement;
                                var ordersElement = GetOrdersArray(root);

                                if (ordersElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var order in ordersElement.EnumerateArray())
                                    {
                                        if (order.ValueKind != JsonValueKind.Object) continue;

                                        if (!IsTodayOrder(order)) continue;

                                        string oid = GetOrderNoOrId(order);
                                        if (string.IsNullOrEmpty(oid)) continue;
                                        if (_seenOrderIds.Contains(oid)) continue;

                                        // Fetch details
                                        var detail = await FetchOrderDetail(client, baseUrl, oid, token);
                                        var finalOrder = MergeOrderDetails(order, detail);

                                        string autoRole = app.AutoPrintRole;
                                        if (string.IsNullOrEmpty(autoRole)) autoRole = "kitchen";

                                        OrderReceived?.Invoke(finalOrder, autoRole);
                                        _seenOrderIds.Add(oid);
                                    }
                                }
                            }
                            backoff = 5; // Reset backoff
                        }
                    }

                    await Task.Delay(10000, token);
                }
                catch (Exception ex)
                {
                    LogMessage($"[API POLL ERROR] {ex.Message}");
                    await Task.Delay(backoff * 1000, token);
                    backoff = Math.Min(backoff * 2, 60);
                }
            }
        }

        private async Task<JsonElement?> FetchOrderDetail(HttpClient client, string baseUrl, string orderId, CancellationToken token)
        {
            string[] candidates = {
                $"{baseUrl}/printer/orders/{orderId}",
                $"{baseUrl}/restaurant/orders/{orderId}",
                $"{baseUrl}/restaurant/order/{orderId}"
            };

            foreach (var url in candidates)
            {
                try
                {
                    var response = await client.GetAsync(url, token);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync(token);
                        var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("data", out var dataProp))
                        {
                            if (dataProp.ValueKind == JsonValueKind.Object)
                            {
                                if (dataProp.TryGetProperty("order", out var orderProp)) return orderProp.Clone();
                                return dataProp.Clone();
                            }
                        }
                        if (root.TryGetProperty("order", out var directOrderProp)) return directOrderProp.Clone();
                        return root.Clone();
                    }
                }
                catch
                {
                    // Continue to next candidate URL
                }
            }
            return null;
        }

        private JsonElement MergeOrderDetails(JsonElement baseOrder, JsonElement? detailOrder)
        {
            if (detailOrder == null) return baseOrder;

            // In C# we can build a combined Dictionary and serialize it back to JsonElement
            var dict = new Dictionary<string, object>();
            
            // Add base properties
            foreach (var prop in baseOrder.EnumerateObject())
            {
                dict[prop.Name] = GetValue(prop.Value);
            }

            // Merge details
            foreach (var prop in detailOrder.Value.EnumerateObject())
            {
                if (!dict.ContainsKey(prop.Name) || IsEmptyValue(dict[prop.Name]))
                {
                    dict[prop.Name] = GetValue(prop.Value);
                }
            }

            string json = JsonSerializer.Serialize(dict);
            using (var doc = JsonDocument.Parse(json))
            {
                return doc.RootElement.Clone();
            }
        }

        private object GetValue(JsonElement el)
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

        private bool IsEmptyValue(object val)
        {
            if (val == null) return true;
            if (val is string s && string.IsNullOrEmpty(s)) return true;
            if (val is List<object> l && l.Count == 0) return true;
            if (val is Dictionary<string, object> d && d.Count == 0) return true;
            return false;
        }

        private JsonElement GetOrdersArray(JsonElement root)
        {
            if (root.TryGetProperty("data", out var dataProp))
            {
                if (dataProp.ValueKind == JsonValueKind.Object)
                {
                    if (dataProp.TryGetProperty("orders", out var ordersProp)) return ordersProp;
                    if (dataProp.TryGetProperty("items", out var itemsProp)) return itemsProp;
                }
                else if (dataProp.ValueKind == JsonValueKind.Array)
                {
                    return dataProp;
                }
            }
            if (root.TryGetProperty("orders", out var rootOrdersProp)) return rootOrdersProp;
            return default;
        }

        private bool IsTodayOrder(JsonElement order)
        {
            if (!order.TryGetProperty("created_at", out var createdProp)) return true;
            string dateStr = createdProp.GetString() ?? "";
            if (string.IsNullOrEmpty(dateStr)) return true;

            try
            {
                if (DateTime.TryParse(dateStr, out DateTime orderDate))
                {
                    return orderDate.Date == DateTime.Today;
                }
            }
            catch
            {
                // Fallback to true if date parsing fails
            }
            return true;
        }

        private string GetOrderNoOrId(JsonElement order)
        {
            if (order.TryGetProperty("id", out var idProp))
            {
                if (idProp.ValueKind == JsonValueKind.Number) return idProp.GetDouble().ToString();
                return idProp.GetString() ?? "";
            }
            if (order.TryGetProperty("order_number", out var numProp)) return numProp.GetString() ?? "";
            return "";
        }

        private void LogMessage(string message)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string logDir = Path.Combine(appData, "nlkCheffiePrint", "logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "ws_client.log");
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}\n";
                LogMaintenance.Append(logFile, line, 2 * 1024 * 1024);
            }
            catch
            {
                // Ignore log errors
            }
        }
    }
}

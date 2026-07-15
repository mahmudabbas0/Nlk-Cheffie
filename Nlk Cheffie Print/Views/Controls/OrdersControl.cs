using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views.Controls
{
    public partial class OrdersControl : UserControl
    {
        // ── Pagination state ─────────────────────────────────────────────
        private const int PageSize     = 15;
        private int       _currentPage = 1;
        private int       _totalPages  = 1;
        private int       _totalRecords = 0;

        // All orders for the selected date (filtered client-side)
        private readonly List<Order> _allOrders = new();

        // Guard: prevent concurrent LoadOrders calls causing duplicate rows
        private bool _isLoading;

        private int _hoveredRowIndex = -1;
        private int _hoveredColumnIndex = -1;

        public OrdersControl()
        {
            InitializeComponent();
            LocalizationService.LanguageChanged += TranslateUI;
            this.DoubleBuffered = true;
        }

        // ────────────────────────────────────────────────────────────────
        //  Lifecycle
        // ────────────────────────────────────────────────────────────────
        private void OrdersControl_Load(object sender, EventArgs e)
        {
            SetupGridColumns();
            ThemeManager.ApplyTheme(this);
            TranslateUI();
            dgvOrders.CellPainting += dgvOrders_CellPainting;
            dgvOrders.CellMouseEnter += dgvOrders_CellMouseEnter;
            dgvOrders.CellMouseLeave += dgvOrders_CellMouseLeave;
            MainForm.OrderListChanged += OnOrderListChanged;

            // Connect FlatScrollBar to DataGridView
            dgvOrders.Scroll += (s, ev) =>
            {
                if (ev.ScrollOrientation == ScrollOrientation.VerticalScroll)
                {
                    flatScrollBar.Value = ev.NewValue;
                }
            };

            flatScrollBar.Scroll += (s, ev) =>
            {
                if (flatScrollBar.Value >= 0 && flatScrollBar.Value < dgvOrders.RowCount)
                {
                    dgvOrders.FirstDisplayedScrollingRowIndex = flatScrollBar.Value;
                }
            };

            dgvOrders.SizeChanged += (s, ev) => UpdateGridScrollBar();

            _ = LoadOrders(1);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LocalizationService.LanguageChanged -= TranslateUI;
            MainForm.OrderListChanged           -= OnOrderListChanged;
            base.OnHandleDestroyed(e);
        }

        // ────────────────────────────────────────────────────────────────
        //  Grid setup
        // ────────────────────────────────────────────────────────────────
        private void SetupGridColumns()
        {
            dgvOrders.Columns.Clear();
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.DataError  += (s, ev) => ev.ThrowException = false;
            dgvOrders.ScrollBars  = ScrollBars.None;

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DateTime", Name = "colDate", HeaderText = "Date",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 15, MinimumWidth = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Section", Name = "colSection", HeaderText = "Section",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 10, MinimumWidth = 55
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TableName", Name = "colTable", HeaderText = "Table",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 9, MinimumWidth = 50
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderNumber", Name = "colOrderNo", HeaderText = "Order No",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 16, MinimumWidth = 80
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WaiterName", Name = "colWaiter", HeaderText = "Waiter",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 9, MinimumWidth = 50
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalAmount", Name = "colAmount", HeaderText = "Amount",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 10, MinimumWidth = 60
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status", Name = "colStatus", HeaderText = "Order Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 14, MinimumWidth = 80
            });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PaymentMethod", Name = "colPayment", HeaderText = "Payment",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 11, MinimumWidth = 70
            });

            var detailBtn = new DataGridViewButtonColumn
            {
                Text = "Detail", UseColumnTextForButtonValue = true,
                Name = "colDetail", HeaderText = "Detail",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 6, MinimumWidth = 55,
                FlatStyle = FlatStyle.Flat
            };
            detailBtn.DefaultCellStyle.BackColor = ThemeManager.ColorFieldBg;
            detailBtn.DefaultCellStyle.ForeColor  = ThemeManager.ColorAccent;
            dgvOrders.Columns.Add(detailBtn);
        }

        // ────────────────────────────────────────────────────────────────
        //  Localisation
        // ────────────────────────────────────────────────────────────────
        public void TranslateUI()
        {
            if (lblTitle == null || dgvOrders == null) return;

            lblTitle.Text = LocalizationService.T("tabs.orders").ToUpper();

            if (lblStatus != null)
            {
                lblStatus.Text      = LocalizationService.T("login.online");
                lblStatus.ForeColor = ThemeManager.ColorSuccess;
            }

            if (dgvOrders.Columns.Count > 0)
            {
                SetColHeader("colDate",    "orders.columns.date");
                SetColHeader("colSection", "orders.columns.type");
                SetColHeader("colTable",   "orders.columns.table");
                SetColHeader("colOrderNo", "orders.columns.order_no");
                SetColHeader("colWaiter",  "orders.columns.waiter");
                SetColHeader("colAmount",  "orders.columns.amount");
                SetColHeader("colStatus",  "orders.columns.status");
                SetColHeader("colPayment", "orders.columns.payment");
                SetColHeader("colDetail",  "orders.detail.dialog_title");
                if (dgvOrders.Columns["colDetail"] is DataGridViewButtonColumn btn)
                    btn.Text = LocalizationService.T("orders.detail.dialog_title", "Detail");
            }

            if (btnApprove != null) btnApprove.Text = LocalizationService.T("orders.actions.accept");
            if (btnCancel  != null) btnCancel.Text  = LocalizationService.T("orders.actions.reject");
            if (btnRefresh != null) btnRefresh.Text = LocalizationService.T("orders.actions.refresh");

            UpdatePaginationUI();
        }

        private void SetColHeader(string colName, string key)
        {
            if (dgvOrders.Columns[colName] != null)
                dgvOrders.Columns[colName]!.HeaderText = LocalizationService.T(key);
        }

        // ────────────────────────────────────────────────────────────────
        //  Pagination
        // ────────────────────────────────────────────────────────────────
        private void UpdatePaginationUI()
        {
            if (lblPageInfo  == null) return;
            lblPageInfo.Text         = $"{_currentPage} / {_totalPages}";
            if (btnPrevPage != null) btnPrevPage.Enabled = _currentPage > 1;
            if (btnNextPage != null) btnNextPage.Enabled = _currentPage < _totalPages;
        }

        private void RefreshGridData()
        {
            // Client-side paging: slice _allOrders
            int skip = (_currentPage - 1) * PageSize;
            int take = Math.Min(PageSize, Math.Max(0, _allOrders.Count - skip));

            var page = _allOrders.GetRange(skip, take);

            // Suspend layout to prevent rendering corruption during data-source swap
            dgvOrders.SuspendLayout();
            dgvOrders.DataSource = null;
            dgvOrders.DataSource = page;
            dgvOrders.ResumeLayout();

            UpdateGridScrollBar();
            UpdatePaginationUI();
        }

        private void UpdateGridScrollBar()
        {
            if (this.IsDisposed) return;
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(UpdateGridScrollBar));
                    return;
                }
                int visibleRows = dgvOrders.DisplayedRowCount(false);
                if (dgvOrders.RowCount > visibleRows)
                {
                    flatScrollBar.Maximum = dgvOrders.RowCount - 1;
                    flatScrollBar.LargeChange = visibleRows;
                    flatScrollBar.Value = dgvOrders.FirstDisplayedScrollingRowIndex;
                    flatScrollBar.Visible = true;
                }
                else
                {
                    flatScrollBar.Visible = false;
                }
            }
            catch { }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage <= 1) return;
            _currentPage--;
            RefreshGridData();
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_currentPage >= _totalPages) return;
            _currentPage++;
            RefreshGridData();
        }

        // ────────────────────────────────────────────────────────────────
        //  Data loading  (client-side date filter + local pagination)
        // ────────────────────────────────────────────────────────────────
        private async Task LoadOrders(int page = 1)
        {
            // Prevent concurrent calls from duplicating rows
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _currentPage = page;
                _allOrders.Clear();

                string deviceToken = ConfigManager.Current.App.DeviceToken;
                string baseUrl     = ConfigManager.Current.App.ApiBaseUrl.TrimEnd('/');
                string slug        = ConfigManager.Current.App.RestaurantSlug;

                if (string.IsNullOrEmpty(deviceToken))
                {
                    _totalPages = _totalRecords = 0;
                    RefreshGridData();
                    return;
                }

                DateTime selectedDate = dtpDate.Value.Date;
                string   dateStr      = selectedDate.ToString("yyyy-MM-dd");
                bool     loadedFromApi = false;

                // ── Fetch from API, filter by date client-side ───────────────
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                    client.DefaultRequestHeaders.Add("Accept",           "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent",       "CheffiePOS-PrintBridge/1.0");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", deviceToken);
                    if (!string.IsNullOrEmpty(slug))
                        client.DefaultRequestHeaders.Add("X-Restaurant-Slug", slug);

                    const int apiPerPage = 100;
                    int apiPage     = 1;
                    int apiLastPage = 1;

                    do
                    {
                        string url = $"{baseUrl}/printer/orders?page={apiPage}&per_page={apiPerPage}";
                        var response = await client.GetAsync(url);
                        if (!response.IsSuccessStatusCode) break;

                        string json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        apiLastPage = ReadPaginationMeta(root, "last_page", 1);

                        var ordersArray = GetOrdersArray(root);
                        if (ordersArray.ValueKind != JsonValueKind.Array) break;

                        bool passedOlder = false;
                        foreach (var o in ordersArray.EnumerateArray())
                        {
                            if (o.ValueKind != JsonValueKind.Object) continue;
                            var order = MapJsonToOrder(o);

                            // API is DESC — stop once we pass the selected date
                            if (order.DateTime.Date < selectedDate) { passedOlder = true; break; }
                            if (order.DateTime.Date == selectedDate) _allOrders.Add(order);
                        }

                        loadedFromApi = true;
                        if (passedOlder) break;
                        apiPage++;
                    }
                    while (apiPage <= apiLastPage);
                }
                catch
                {
                    // Network failure — fall through to local logs
                }

                // ── Fallback: local log file ─────────────────────────────────
                if (!loadedFromApi)
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string logPath = string.IsNullOrEmpty(slug)
                        ? Path.Combine(appData, "nlkCheffiePrint", "logs", $"orders_stream_{dateStr}.log")
                        : Path.Combine(appData, "nlkCheffiePrint", "logs", $"orders_stream_{slug}_{dateStr}.log");

                    if (File.Exists(logPath))
                    {
                        try
                        {
                            foreach (string line in File.ReadAllLines(logPath))
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;
                                using var doc = JsonDocument.Parse(line);
                                _allOrders.Add(MapJsonToOrder(doc.RootElement));
                            }
                        }
                        catch { /* ignore parse errors */ }
                    }
                }

                // ── Compute local pagination ─────────────────────────────────
                _totalRecords = _allOrders.Count;
                _totalPages   = Math.Max(1, (int)Math.Ceiling(_totalRecords / (double)PageSize));
                if (_currentPage > _totalPages) _currentPage = _totalPages;

                RefreshGridData();
            }
            finally
            {
                _isLoading = false; // Always release the guard
            }
        }


        // ────────────────────────────────────────────────────────────────
        //  JSON helpers
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads a pagination integer from:
        ///   data.pagination.key  (primary API shape)
        ///   data.key             (flat inside data)
        ///   root.key             (root fallback)
        ///   meta.key             (Laravel meta)
        /// </summary>
        private static int ReadPaginationMeta(JsonElement root, string key, int fallback)
        {
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                if (data.TryGetProperty("pagination", out var pag) && pag.ValueKind == JsonValueKind.Object
                    && pag.TryGetProperty(key, out var pp) && pp.ValueKind == JsonValueKind.Number)
                    return pp.GetInt32();

                if (data.TryGetProperty(key, out var dp) && dp.ValueKind == JsonValueKind.Number)
                    return dp.GetInt32();
            }
            if (root.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.Number)
                return p.GetInt32();
            if (root.TryGetProperty("meta", out var meta) && meta.ValueKind == JsonValueKind.Object
                && meta.TryGetProperty(key, out var mp) && mp.ValueKind == JsonValueKind.Number)
                return mp.GetInt32();

            return fallback;
        }

        /// <summary>
        /// Extracts the orders array from various API response shapes.
        /// Primary shape: { success, data: { orders:[...], pagination:{...} } }
        /// </summary>
        private static JsonElement GetOrdersArray(JsonElement root)
        {
            if (root.TryGetProperty("data", out var data))
            {
                if (data.ValueKind == JsonValueKind.Object)
                {
                    if (data.TryGetProperty("orders", out var op) && op.ValueKind == JsonValueKind.Array) return op;
                    if (data.TryGetProperty("items",  out var ip) && ip.ValueKind == JsonValueKind.Array) return ip;
                    if (data.TryGetProperty("data",   out var dd) && dd.ValueKind == JsonValueKind.Array) return dd;
                }
                else if (data.ValueKind == JsonValueKind.Array)
                    return data;
            }
            if (root.TryGetProperty("orders", out var ro)) return ro;
            return default;
        }

        /// <summary>
        /// Maps a single JSON order element to an Order model.
        /// Supports both the REST API shape (OrderResource) and the WebSocket/log shape.
        /// </summary>
        private static Order MapJsonToOrder(JsonElement root)
        {
            var order = new Order
            {
                OrderNumber     = GetJsonStr(root, "order_number"),
                Status          = TranslateStatus(GetJsonStr(root, "status")),
                PaymentMethod   = TranslatePaymentMethod(GetJsonStr(root, "payment_method")),
                PaymentStatus   = GetJsonStr(root, "payment_status"),
                OrderNote       = GetJsonStr(root, "notes"),
                CustomerName    = GetJsonStr(root, "customer_name"),
                CustomerPhone   = GetJsonStr(root, "customer_phone"),
                CustomerEmail   = GetJsonStr(root, "customer_email"),
                DeliveryAddress = GetJsonStr(root, "delivery_address"),
            };
            if (string.IsNullOrEmpty(order.CustomerEmail)) order.CustomerEmail = GetJsonStr(root, "email");
            if (string.IsNullOrEmpty(order.PaymentStatus)) order.PaymentStatus = GetJsonStr(root, "payment_state");

            // Total: try total_amount (API), then subtotal, then total (legacy)
            string totalRaw = GetJsonStr(root, "total_amount");
            if (string.IsNullOrEmpty(totalRaw)) totalRaw = GetJsonStr(root, "subtotal");
            if (string.IsNullOrEmpty(totalRaw)) totalRaw = GetJsonStr(root, "total");
            order.TotalAmount = string.IsNullOrEmpty(totalRaw) ? "" : $"{totalRaw} TL";

            // Table: API returns an object { id, name, ... }
            if (root.TryGetProperty("table", out var tableEl))
            {
                order.TableName = tableEl.ValueKind == JsonValueKind.Object
                    ? GetJsonStr(tableEl, "name")
                    : (tableEl.ValueKind == JsonValueKind.String ? tableEl.GetString() ?? "" : "");
            }
            if (string.IsNullOrEmpty(order.TableName)) order.TableName = GetJsonStr(root, "table_name");

            // Waiter / Section (not in API OrderResource — WebSocket / log shape)
            order.WaiterName = GetJsonStr(root, "waiter");
            order.Section    = TranslateSection(GetJsonStr(root, "role"));

            // order_info fallback (WebSocket/log shape)
            if (root.TryGetProperty("order_info", out var oi) && oi.ValueKind == JsonValueKind.Object)
            {
                if (string.IsNullOrEmpty(order.OrderNumber))   order.OrderNumber   = GetJsonStr(oi, "order_number");
                if (string.IsNullOrEmpty(order.TableName))     order.TableName     = GetJsonStr(oi, "table_name");
                if (string.IsNullOrEmpty(order.WaiterName))    order.WaiterName    = GetJsonStr(oi, "waiter_name");
                if (string.IsNullOrEmpty(order.Section))       order.Section       = TranslateSection(GetJsonStr(oi, "section"));
                if (string.IsNullOrEmpty(order.Status))        order.Status        = TranslateStatus(GetJsonStr(oi, "status"));
                if (string.IsNullOrEmpty(order.PaymentMethod)) order.PaymentMethod = TranslatePaymentMethod(GetJsonStr(oi, "payment_method"));
                if (string.IsNullOrEmpty(order.PaymentStatus)) order.PaymentStatus = GetJsonStr(oi, "payment_status");
                if (string.IsNullOrEmpty(order.PaymentStatus)) order.PaymentStatus = GetJsonStr(oi, "payment_state");
                if (string.IsNullOrEmpty(order.OrderNote))     order.OrderNote     = GetJsonStr(oi, "order_note");
                if (string.IsNullOrEmpty(order.CustomerName))  order.CustomerName  = GetJsonStr(oi, "customer_name");
                if (string.IsNullOrEmpty(order.CustomerPhone)) order.CustomerPhone = GetJsonStr(oi, "customer_phone");
                if (string.IsNullOrEmpty(order.CustomerEmail)) order.CustomerEmail = GetJsonStr(oi, "customer_email");
                if (string.IsNullOrEmpty(order.CustomerEmail)) order.CustomerEmail = GetJsonStr(oi, "email");
                if (string.IsNullOrEmpty(order.DeliveryAddress)) order.DeliveryAddress = GetJsonStr(oi, "delivery_address");
                if (string.IsNullOrEmpty(order.TotalAmount))   order.TotalAmount   = GetJsonStr(oi, "total") + " TL";
            }

            // Date: try created_at (API), then ts (WebSocket), then order_info.date
            string ts = "";
            if (root.TryGetProperty("created_at", out var cat))
                ts = cat.ValueKind == JsonValueKind.String ? cat.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(ts)) ts = GetJsonStr(root, "ts");
            if (string.IsNullOrEmpty(ts) &&
                root.TryGetProperty("order_info", out var oi3) && oi3.ValueKind == JsonValueKind.Object &&
                oi3.TryGetProperty("date", out var d))
                ts = $"{d.GetString()} {GetJsonStr(oi3, "time")}";

            if (DateTime.TryParse(ts, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
            {
                order.DateTime = ts.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ? dt.ToUniversalTime() : dt;
            }
            else
            {
                order.DateTime = DateTime.Now;
            }

            // Items: order_items (API) or items (WebSocket/log)
            JsonElement itemsEl = default;
            if (root.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                itemsEl = apiItems;
            else if (root.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                itemsEl = wsItems;

            if (itemsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsEl.EnumerateArray())
                {
                    int qty = 1;
                    if (item.TryGetProperty("quantity", out var q) && q.ValueKind == JsonValueKind.Number)
                        qty = q.GetInt32();

                    // Parse name (multilingual and nested support)
                    string name = GetJsonStr(item, "name");
                    if (string.IsNullOrEmpty(name)) name = GetJsonStr(item, "product_name");
                    if (string.IsNullOrEmpty(name) && item.TryGetProperty("product", out var prodObj) && prodObj.ValueKind == JsonValueKind.Object)
                    {
                        if (prodObj.TryGetProperty("name", out var prodNames))
                        {
                            if (prodNames.ValueKind == JsonValueKind.String)
                            {
                                name = prodNames.GetString() ?? "";
                            }
                            else if (prodNames.ValueKind == JsonValueKind.Object)
                            {
                                string language = ConfigManager.Current.App.Language;
                                name = GetJsonStr(prodNames, language);
                                if (string.IsNullOrEmpty(name)) name = GetJsonStr(prodNames, "tr");
                                if (string.IsNullOrEmpty(name)) name = GetJsonStr(prodNames, "en");
                            }
                        }
                    }

                    // Parse total/price
                    string lineTotal = GetJsonStr(item, "subtotal");
                    if (string.IsNullOrEmpty(lineTotal)) lineTotal = GetJsonStr(item, "line_total");
                    if (string.IsNullOrEmpty(lineTotal))
                    {
                        string priceStr = GetJsonStr(item, "price");
                        if (double.TryParse(priceStr, out double priceVal))
                        {
                            lineTotal = (priceVal * qty).ToString("0.00");
                        }
                    }
                    if (string.IsNullOrEmpty(lineTotal))
                    {
                        if (item.TryGetProperty("product", out var prodObjForPrice) && prodObjForPrice.ValueKind == JsonValueKind.Object)
                        {
                            string priceStr = GetJsonStr(prodObjForPrice, "price");
                            if (double.TryParse(priceStr, out double priceVal))
                            {
                                lineTotal = (priceVal * qty).ToString("0.00");
                            }
                        }
                    }

                    // Parse customizations (extras)
                    var addedCust = new List<string>();

                    // 1. Try Laravel "extras" relation array
                    if (item.TryGetProperty("extras", out var extrasEl) && extrasEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var ex in extrasEl.EnumerateArray())
                        {
                            string exName = GetJsonStr(ex, "name");
                            if (string.IsNullOrEmpty(exName)) exName = GetJsonStr(ex, "option_name");

                            string exPrice = GetJsonStr(ex, "price");
                            if (double.TryParse(exPrice, out double exPriceVal) && exPriceVal > 0)
                            {
                                exName += $" (+{exPriceVal:0.00} TL)";
                            }
                            if (!string.IsNullOrEmpty(exName) && !addedCust.Contains(exName))
                            {
                                addedCust.Add(exName);
                            }
                        }
                    }

                    // 2. Try standard customizations
                    if (item.TryGetProperty("customizations", out var custEl))
                    {
                        if (custEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var c in custEl.EnumerateArray())
                            {
                                string cName = c.ValueKind == JsonValueKind.String ? c.GetString() ?? "" : GetJsonStr(c, "name");
                                if (c.ValueKind == JsonValueKind.Object)
                                {
                                    string cPrice = GetJsonStr(c, "price");
                                    if (double.TryParse(cPrice, out double cPriceVal) && cPriceVal > 0)
                                    {
                                        cName += $" (+{cPriceVal:0.00} TL)";
                                    }
                                }
                                if (!string.IsNullOrEmpty(cName) && !addedCust.Contains(cName))
                                {
                                    addedCust.Add(cName);
                                }
                            }
                        }
                        else if (custEl.ValueKind == JsonValueKind.Object && custEl.TryGetProperty("added", out var addedEl) && addedEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var c in addedEl.EnumerateArray())
                            {
                                string cName = c.ValueKind == JsonValueKind.String ? c.GetString() ?? "" : GetJsonStr(c, "name");
                                if (c.ValueKind == JsonValueKind.Object)
                                {
                                    string cPrice = GetJsonStr(c, "price");
                                    if (double.TryParse(cPrice, out double cPriceVal) && cPriceVal > 0)
                                    {
                                        cName += $" (+{cPriceVal:0.00} TL)";
                                    }
                                }
                                if (!string.IsNullOrEmpty(cName) && !addedCust.Contains(cName))
                                {
                                    addedCust.Add(cName);
                                }
                            }
                        }
                    }

                    order.Items.Add(new OrderItem
                    {
                        Name                 = name,
                        Quantity             = qty,
                        UnitPrice            = GetJsonStr(item, "price"),
                        LineTotal            = lineTotal,
                        Notes                = GetJsonStr(item, "notes"),
                        AddedCustomizations  = addedCust
                    });
                }
            }

            // Parse subtotal, tax, and extras_total
            string sub = GetJsonStr(root, "subtotal");
            string tx = GetJsonStr(root, "tax");
            if (string.IsNullOrEmpty(tx)) tx = GetJsonStr(root, "tax_amount");
            string ext = GetJsonStr(root, "extras_total");

            if (root.TryGetProperty("payment_info", out var pInfo) && pInfo.ValueKind == JsonValueKind.Object)
            {
                if (string.IsNullOrEmpty(sub)) sub = GetJsonStr(pInfo, "subtotal");
                if (string.IsNullOrEmpty(tx))  tx  = GetJsonStr(pInfo, "tax");
                if (string.IsNullOrEmpty(ext)) ext = GetJsonStr(pInfo, "extras_total");
            }
            if (root.TryGetProperty("order_info", out var oInfo) && oInfo.ValueKind == JsonValueKind.Object)
            {
                if (string.IsNullOrEmpty(sub)) sub = GetJsonStr(oInfo, "subtotal");
                if (string.IsNullOrEmpty(tx))  tx  = GetJsonStr(oInfo, "tax");
                if (string.IsNullOrEmpty(ext)) ext = GetJsonStr(oInfo, "extras_total");
            }

            order.Subtotal = string.IsNullOrEmpty(sub) ? "0.00" : sub;
            order.Tax = string.IsNullOrEmpty(tx) ? "0.00" : tx;
            order.ExtrasTotal = string.IsNullOrEmpty(ext) ? "0.00" : ext;

            return order;
        }

        private static string GetJsonStr(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var p))
                return p.ValueKind == JsonValueKind.Number ? p.GetDouble().ToString() : p.GetString() ?? "";
            return "";
        }

        // ────────────────────────────────────────────────────────────────
        //  Translators
        // ────────────────────────────────────────────────────────────────
        private static string TranslateSection(string role) => role.ToLower() switch
        {
            "kitchen" => LocalizationService.T("settings.kitchen"),
            "cashier" => LocalizationService.T("settings.cashier"),
            "courier" => LocalizationService.T("settings.courier"),
            _         => role
        };

        private static string TranslateStatus(string status) => status.ToLower() switch
        {
            "pending"    => LocalizationService.T("orders.status.pending"),
            "accepted"   => LocalizationService.T("orders.status.accepted"),
            "confirmed"  => LocalizationService.T("orders.status.accepted"),
            "preparing"  => LocalizationService.T("orders.status.preparing"),
            "ready"      => LocalizationService.T("orders.status.ready"),
            "on_the_way" => LocalizationService.T("orders.status.on_the_way"),
            "delivered"  => LocalizationService.T("orders.status.delivered"),
            "canceled"   => LocalizationService.T("orders.status.canceled"),
            _            => status
        };

        private static string TranslatePaymentMethod(string method) => method.ToLower() switch
        {
            "cash"   => LocalizationService.T("payment.methods.cash"),
            "card"   => LocalizationService.T("payment.methods.card"),
            "online" => LocalizationService.T("payment.methods.online"),
            _        => method
        };

        // ────────────────────────────────────────────────────────────────
        //  Event handlers
        // ────────────────────────────────────────────────────────────────
        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            MessageBox.Show(
                LocalizationService.T("orders.dialogs.print_queued"),
                LocalizationService.T("orders.actions.accept"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            foreach (DataGridViewRow row in dgvOrders.SelectedRows)
                if (row.DataBoundItem is Order o)
                    o.Status = LocalizationService.T("orders.status.accepted");

            RefreshGridData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            if (MessageBox.Show(
                    LocalizationService.T("orders.dialogs.reject_msg"),
                    LocalizationService.T("orders.dialogs.reject_title"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            foreach (DataGridViewRow row in dgvOrders.SelectedRows)
                if (row.DataBoundItem is Order o)
                    o.Status = LocalizationService.T("orders.status.canceled");

            RefreshGridData();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
            => _ = LoadOrders(1);

        private void dtpDate_ValueChanged(object sender, EventArgs e)
            => _ = LoadOrders(1);

        private void dgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "colDetail"
                && dgvOrders.Rows[e.RowIndex].DataBoundItem is Order order)
                ShowOrderDetail(order);
        }

        private void dgvOrders_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "colDetail")
            {
                _hoveredRowIndex = e.RowIndex;
                _hoveredColumnIndex = e.ColumnIndex;
                dgvOrders.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void dgvOrders_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "colDetail")
            {
                int oldRow = _hoveredRowIndex;
                int oldCol = _hoveredColumnIndex;
                _hoveredRowIndex = -1;
                _hoveredColumnIndex = -1;
                if (oldRow == e.RowIndex && oldCol == e.ColumnIndex)
                {
                    dgvOrders.InvalidateCell(oldCol, oldRow);
                }
            }
        }

        private static void ShowOrderDetail(Order order)
        {
            using var preview = new ReceiptPreviewForm(order);
            preview.ShowDialog();
        }

        private void OnOrderListChanged()
        {
            if (InvokeRequired)
                Invoke(() => _ = LoadOrders(1));
            else
                _ = LoadOrders(1);
        }

        private static string TranslatePaymentStatus(string status) => status.ToLowerInvariant() switch
        {
            "pending" => LocalizationService.T("payment.status.pending", "Bekliyor"),
            "paid"    => LocalizationService.T("payment.status.paid", "Ödendi"),
            "waiting" => LocalizationService.T("payment.status.pending", "Bekliyor"),
            "error"   => LocalizationService.T("payment.status.error", "Hata"),
            "refunded"=> LocalizationService.T("payment.status.refunded", "İade"),
            _         => status
        };

        private void dgvOrders_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (colName == "colPayment")
                {
                    // Draw background
                    e.PaintBackground(e.CellBounds, true);

                    if (dgvOrders.Rows[e.RowIndex].DataBoundItem is Order order)
                    {
                        // Get values
                        string methodStr = TranslatePaymentMethod(order.PaymentMethod);
                        string statusStr = TranslatePaymentStatus(order.PaymentStatus);

                        // Colors: method is Green, status is Orange/Yellow
                        Color methodColor = Color.FromArgb(76, 175, 80); // Green
                        Color statusColor = Color.FromArgb(255, 152, 0); // Orange/Yellow

                        // Calculate layout bounds
                        int cellHeight = e.CellBounds.Height;
                        int textHeight = 32; // height of both lines + spacing
                        int x = e.CellBounds.X + 8;
                        int y = e.CellBounds.Y + (cellHeight - textHeight) / 2;

                        // Draw first line (Method)
                        using (var brushMethod = new SolidBrush(methodColor))
                        {
                            e.Graphics.DrawString(methodStr, ThemeManager.FontBodyBold, brushMethod, x, y);
                        }

                        // Draw second line (Status)
                        using (var brushStatus = new SolidBrush(statusColor))
                        {
                            e.Graphics.DrawString(statusStr, ThemeManager.FontBody, brushStatus, x, y + 16);
                        }
                    }

                    e.Handled = true;
                }
                else if (colName == "colDetail")
                {
                    // Paint cell background
                    e.PaintBackground(e.CellBounds, true);

                    // Draw a nice premium flat button in the cell
                    int padX = 8;
                    int padY = 8;
                    var btnBounds = new Rectangle(e.CellBounds.X + padX, e.CellBounds.Y + padY, e.CellBounds.Width - (padX * 2), e.CellBounds.Height - (padY * 2));

                    // Use dark card color, or orange highlight if hovered, or gray if selected
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                    bool isHovered = (e.RowIndex == _hoveredRowIndex && e.ColumnIndex == _hoveredColumnIndex);

                    Color btnBg;
                    if (isHovered)
                        btnBg = Color.FromArgb(70, ThemeManager.ColorAccent); // Glow accent orange on hover
                    else if (isSelected)
                        btnBg = Color.FromArgb(56, 56, 58);
                    else
                        btnBg = ThemeManager.ColorCard;

                    using (var brushBg = new SolidBrush(btnBg))
                    {
                        e.Graphics.FillRectangle(brushBg, btnBounds);
                    }

                    // Draw thin border
                    using (var penBorder = new Pen(Color.FromArgb(40, Color.White), 1))
                    {
                        e.Graphics.DrawRectangle(penBorder, btnBounds);
                    }

                    // Draw "Detay" button text
                    string btnText = LocalizationService.T("orders.detail.dialog_title", "Detay");
                    using (var brushText = new SolidBrush(isHovered ? Color.White : ThemeManager.ColorAccent))
                    {
                        var size = e.Graphics.MeasureString(btnText, ThemeManager.FontBodyBold);
                        float tx = btnBounds.X + (btnBounds.Width - size.Width) / 2;
                        float ty = btnBounds.Y + (btnBounds.Height - size.Height) / 2;
                        e.Graphics.DrawString(btnText, ThemeManager.FontBodyBold, brushText, tx, ty);
                    }

                    e.Handled = true;
                }
            }
        }
    }
}

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
using Nlk_Cheffie_Print.Core.Net;
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
            
            // Allow check boxes to be editable, but make other columns read-only
            dgvOrders.ReadOnly = false;
            foreach (DataGridViewColumn col in dgvOrders.Columns)
            {
                if (col.Name != "colCheck")
                {
                    col.ReadOnly = true;
                }
            }

            dgvOrders.CellPainting += dgvOrders_CellPainting;
            dgvOrders.CellMouseEnter += dgvOrders_CellMouseEnter;
            dgvOrders.CellMouseLeave += dgvOrders_CellMouseLeave;
            dgvOrders.CellFormatting += dgvOrders_CellFormatting;
            dgvOrders.CellClick += dgvOrders_CellClick;
            dgvOrders.CurrentCellDirtyStateChanged += dgvOrders_CurrentCellDirtyStateChanged;
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
            dgvOrders.CellFormatting            -= dgvOrders_CellFormatting;
            dgvOrders.CellClick                 -= dgvOrders_CellClick;
            dgvOrders.CurrentCellDirtyStateChanged -= dgvOrders_CurrentCellDirtyStateChanged;
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

            // Add Checkbox Column at index 0 for Multi-Select
            var checkCol = new DataGridViewCheckBoxColumn
            {
                Name = "colCheck",
                HeaderText = "",
                Width = 40,
                MinimumWidth = 40,
                Resizable = DataGridViewTriState.False,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat
            };
            checkCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(checkCol);

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
            if (btnSelectAll != null)
            {
                btnSelectAll.Text = _allSelectedState
                    ? LocalizationService.T("orders.actions.deselect_all", "Seçimi Kaldır")
                    : LocalizationService.T("orders.actions.select_all", "Tümünü Seç");
            }

            UpdatePaginationUI();

            dgvOrders.Invalidate();
            dgvOrders.Update();
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
                Id              = GetJsonStr(root, "id"),
                OrderNumber     = GetJsonStr(root, "order_number"),
                Status          = GetJsonStr(root, "status"),
                PaymentMethod   = GetJsonStr(root, "payment_method"),
                PaymentStatus   = GetJsonStr(root, "payment_status"),
                OrderNote       = GetJsonStr(root, "notes"),
                CustomerName    = GetJsonStr(root, "customer_name"),
                CustomerPhone   = GetJsonStr(root, "customer_phone"),
                CustomerEmail   = GetJsonStr(root, "customer_email"),
                DeliveryAddress = GetJsonStr(root, "delivery_address"),
            };
            if (string.IsNullOrWhiteSpace(order.Id) && root.TryGetProperty("id", out var idProp))
            {
                order.Id = idProp.ValueKind == JsonValueKind.Number
                    ? idProp.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : idProp.GetString() ?? "";
            }
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
            order.Section    = GetJsonStr(root, "role");

            // order_info fallback (WebSocket/log shape)
            if (root.TryGetProperty("order_info", out var oi) && oi.ValueKind == JsonValueKind.Object)
            {
                if (string.IsNullOrEmpty(order.OrderNumber))   order.OrderNumber   = GetJsonStr(oi, "order_number");
                if (string.IsNullOrEmpty(order.TableName))     order.TableName     = GetJsonStr(oi, "table_name");
                if (string.IsNullOrEmpty(order.WaiterName))    order.WaiterName    = GetJsonStr(oi, "waiter_name");
                if (string.IsNullOrEmpty(order.Section))       order.Section       = GetJsonStr(oi, "section");
                if (string.IsNullOrEmpty(order.Status))        order.Status        = GetJsonStr(oi, "status");
                if (string.IsNullOrEmpty(order.PaymentMethod)) order.PaymentMethod = GetJsonStr(oi, "payment_method");
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
                    string unitPrice = GetJsonStr(item, "unit_price");
                    if (string.IsNullOrEmpty(unitPrice)) unitPrice = GetJsonStr(item, "price");

                    string lineTotal = GetJsonStr(item, "total_price");
                    if (string.IsNullOrEmpty(lineTotal)) lineTotal = GetJsonStr(item, "subtotal");
                    if (string.IsNullOrEmpty(lineTotal)) lineTotal = GetJsonStr(item, "line_total");
                    if (string.IsNullOrEmpty(lineTotal) &&
                        double.TryParse(unitPrice, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double unitPriceVal))
                    {
                        lineTotal = (unitPriceVal * qty).ToString("0.00");
                    }
                    if (string.IsNullOrEmpty(lineTotal))
                    {
                        if (item.TryGetProperty("product", out var prodObjForPrice) && prodObjForPrice.ValueKind == JsonValueKind.Object)
                        {
                            string priceStr = GetJsonStr(prodObjForPrice, "price");
                            if (double.TryParse(priceStr, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double priceVal))
                            {
                                lineTotal = (priceVal * qty).ToString("0.00");
                            }
                        }
                    }

                    var addedCust = OrderItemExtrasParser.ParseAddedExtras(item);
                    var removedCust = OrderItemExtrasParser.ParseRemovedExtras(item);

                    order.Items.Add(new OrderItem
                    {
                        ProductId            = OrderItemExtrasParser.GetProductId(item),
                        Name                 = name,
                        Quantity             = qty,
                        UnitPrice            = unitPrice,
                        LineTotal            = lineTotal,
                        Notes                = GetJsonStr(item, "notes"),
                        AddedCustomizations  = addedCust,
                        RemovedCustomizations = removedCust
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

            OrderItemExtrasParser.EnrichOrderFromJson(order, root);
            if (ProductExtrasCatalog.IsLoaded)
                OrderItemExtrasParser.RefreshOrderExtraNames(order);

            return order;
        }

        private static string GetJsonStr(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var p))
            {
                return p.ValueKind switch
                {
                    JsonValueKind.String => p.GetString() ?? "",
                    JsonValueKind.Number => p.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    JsonValueKind.Object => p.GetRawText(),
                    JsonValueKind.Array => p.GetRawText(),
                    _ => ""
                };
            }
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
            dgvOrders.EndEdit();
            var targetOrders = new List<Order>();

            // Collect checked rows
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (row.Cells["colCheck"] != null && Convert.ToBoolean(row.Cells["colCheck"].Value))
                {
                    if (row.DataBoundItem is Order o)
                    {
                        targetOrders.Add(o);
                    }
                }
            }

            // Fallback to selected rows if no checkboxes are checked
            if (targetOrders.Count == 0)
            {
                foreach (DataGridViewRow row in dgvOrders.SelectedRows)
                {
                    if (row.DataBoundItem is Order o)
                    {
                        targetOrders.Add(o);
                    }
                }
            }

            if (targetOrders.Count == 0) return;

            MessageBox.Show(
                LocalizationService.T("orders.dialogs.print_queued"),
                LocalizationService.T("orders.actions.accept"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            foreach (var o in targetOrders)
                o.Status = "accepted";

            RefreshGridData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dgvOrders.EndEdit();
            var targetOrders = new List<Order>();

            // Collect checked rows
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (row.Cells["colCheck"] != null && Convert.ToBoolean(row.Cells["colCheck"].Value))
                {
                    if (row.DataBoundItem is Order o)
                    {
                        targetOrders.Add(o);
                    }
                }
            }

            // Fallback to selected rows if no checkboxes are checked
            if (targetOrders.Count == 0)
            {
                foreach (DataGridViewRow row in dgvOrders.SelectedRows)
                {
                    if (row.DataBoundItem is Order o)
                    {
                        targetOrders.Add(o);
                    }
                }
            }

            if (targetOrders.Count == 0) return;

            if (ConfirmDialog.Show(
                    this,
                    LocalizationService.T("orders.dialogs.reject_title"),
                    LocalizationService.T("orders.dialogs.reject_msg")) != DialogResult.Yes) return;

            foreach (var o in targetOrders)
                o.Status = "canceled";

            RefreshGridData();
        }

        private bool _allSelectedState = false;
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            _allSelectedState = !_allSelectedState;

            if (btnSelectAll != null)
            {
                btnSelectAll.Text = _allSelectedState
                    ? LocalizationService.T("orders.actions.deselect_all", "Seçimi Kaldır")
                    : LocalizationService.T("orders.actions.select_all", "Tümünü Seç");
            }

            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (row.Cells["colCheck"] != null)
                {
                    row.Cells["colCheck"].Value = _allSelectedState;
                }
            }

            dgvOrders.EndEdit();
            dgvOrders.Invalidate();
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
            => _ = LoadOrders(1);

        private void dgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "colDetail"
                && dgvOrders.Rows[e.RowIndex].DataBoundItem is Order order)
                _ = ShowOrderDetailAsync(order);
        }

        private void dgvOrders_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (colName != "colDetail")
                {
                    dgvOrders.EndEdit();
                    var cell = dgvOrders.Rows[e.RowIndex].Cells["colCheck"];
                    bool isChecked = Convert.ToBoolean(cell.Value);
                    cell.Value = !isChecked;
                    dgvOrders.EndEdit();
                    dgvOrders.InvalidateRow(e.RowIndex);
                }
            }
        }

        private void dgvOrders_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvOrders.IsCurrentCellDirty)
            {
                dgvOrders.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
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

        private async Task ShowOrderDetailAsync(Order order)
        {
            Order detailOrder = order;
            try
            {
                await ProductExtrasCatalog.EnsureLoadedAsync();

                string orderKey = !string.IsNullOrWhiteSpace(order.Id) ? order.Id : order.OrderNumber;
                if (!string.IsNullOrWhiteSpace(orderKey))
                {
                    using var client = OrderApiClient.CreateAuthenticatedClient();
                    var detailJson = await OrderApiClient.FetchOrderDetailAsync(client, orderKey);
                    if (detailJson.HasValue)
                    {
                        detailOrder = MapJsonToOrder(detailJson.Value);
                        detailOrder.Id = string.IsNullOrWhiteSpace(detailOrder.Id) ? order.Id : detailOrder.Id;
                        detailOrder.Section = string.IsNullOrWhiteSpace(detailOrder.Section) ? order.Section : detailOrder.Section;
                        detailOrder.WaiterName = string.IsNullOrWhiteSpace(detailOrder.WaiterName) ? order.WaiterName : detailOrder.WaiterName;
                    }
                }

                OrderItemExtrasParser.RefreshOrderExtraNames(detailOrder);
            }
            catch
            {
                // Fall back to list data if detail fetch fails.
            }

            using var preview = new ReceiptPreviewForm(detailOrder);
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

        private void dgvOrders_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Dynamic cell background driven by checkbox value
                bool isChecked = false;
                var checkCell = dgvOrders.Rows[e.RowIndex].Cells["colCheck"];
                if (checkCell != null && checkCell.Value != null && checkCell.Value != DBNull.Value)
                {
                    isChecked = Convert.ToBoolean(checkCell.Value);
                }

                if (isChecked)
                {
                    e.CellStyle.BackColor = Color.FromArgb(44, 44, 46);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(44, 44, 46);
                }
                else
                {
                    e.CellStyle.BackColor = e.RowIndex % 2 == 0 ? dgvOrders.DefaultCellStyle.BackColor : dgvOrders.AlternatingRowsDefaultCellStyle.BackColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor; // Neutralize default Windows selection color
                }

                string colName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (colName == "colSection")
                {
                    if (e.Value is string val)
                    {
                        e.Value = TranslateSection(val);
                        e.FormattingApplied = true;
                    }
                }
                else if (colName == "colStatus")
                {
                    if (e.Value is string val)
                    {
                        e.Value = TranslateStatus(val);
                        e.FormattingApplied = true;

                        Color color = Color.White;
                        string lowerVal = val.ToLowerInvariant();
                        if (lowerVal == "accepted" || lowerVal == "confirmed" || lowerVal.Contains("onay"))
                            color = Color.FromArgb(46, 204, 113); // Vibrant Green
                        else if (lowerVal == "canceled" || lowerVal == "rejected" || lowerVal.Contains("iptal"))
                            color = Color.FromArgb(231, 76, 60); // Vibrant Red
                        else if (lowerVal == "pending" || lowerVal.Contains("bekle"))
                            color = Color.FromArgb(241, 196, 15); // Vibrant Yellow/Orange
                        else if (lowerVal == "preparing" || lowerVal.Contains("hazır"))
                            color = Color.FromArgb(52, 152, 219); // Bright Blue
                        else if (lowerVal == "ready")
                            color = Color.FromArgb(155, 89, 182); // Bright Purple
                        else if (lowerVal == "on_the_way" || lowerVal == "delivered")
                            color = Color.FromArgb(26, 188, 156); // Turquoise Teal

                        e.CellStyle.ForeColor = color;
                        e.CellStyle.SelectionForeColor = color;
                    }
                }
            }
        }

        private void dgvOrders_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string colName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (colName == "colCheck")
                {
                    // Paint background first
                    e.PaintBackground(e.CellBounds, true);

                    // Check if selected or hovered
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

                    // Get checkbox state
                    bool isChecked = false;
                    if (e.Value != null && e.Value != DBNull.Value)
                    {
                        isChecked = Convert.ToBoolean(e.Value);
                    }

                    // Size and location of the checkbox rectangle
                    int boxSize = 18;
                    int bx = e.CellBounds.X + (e.CellBounds.Width - boxSize) / 2;
                    int by = e.CellBounds.Y + (e.CellBounds.Height - boxSize) / 2;
                    var boxRect = new Rectangle(bx, by, boxSize, boxSize);

                    if (e.Graphics != null)
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        if (isChecked)
                        {
                            // Checked: fill with orange and draw a bold checkmark
                            using (var fillBrush = new SolidBrush(ThemeManager.ColorAccent))
                            {
                                e.Graphics.FillRectangle(fillBrush, boxRect);
                            }

                            // Draw a nice white checkmark (✓) inside
                            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                            using (var brushText = new SolidBrush(Color.Black)) // Black text on Orange check looks very clean
                            {
                                var size = e.Graphics.MeasureString("✓", font);
                                float tx = bx + (boxSize - size.Width) / 2 + 1;
                                float ty = by + (boxSize - size.Height) / 2 + 1;
                                e.Graphics.DrawString("✓", font, brushText, tx, ty);
                            }
                        }
                        else
                        {
                            // Unchecked: empty square with light gray border
                            Color borderCol = Color.FromArgb(120, 120, 125);
                            using (var pen = new Pen(borderCol, 1.5f))
                            {
                                e.Graphics.DrawRectangle(pen, boxRect);
                            }
                        }

                        // Draw orange left indicator bar if row is checked
                        if (isChecked)
                        {
                            using (var brush = new SolidBrush(ThemeManager.ColorAccent))
                            {
                                e.Graphics.FillRectangle(brush, e.CellBounds.X, e.CellBounds.Y, 4, e.CellBounds.Height);
                            }
                        }
                    }

                    e.Handled = true;
                }
                else if (colName == "colPayment")
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

                        if (e.Graphics != null)
                        {
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

                    if (e.Graphics != null)
                    {
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
                    }

                    e.Handled = true;
                }
            }
        }
    }
}

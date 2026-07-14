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
                OrderNote       = GetJsonStr(root, "notes"),
                CustomerName    = GetJsonStr(root, "customer_name"),
                CustomerPhone   = GetJsonStr(root, "customer_phone"),
                DeliveryAddress = GetJsonStr(root, "delivery_address"),
            };

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
                if (string.IsNullOrEmpty(order.OrderNote))     order.OrderNote     = GetJsonStr(oi, "order_note");
                if (string.IsNullOrEmpty(order.CustomerName))  order.CustomerName  = GetJsonStr(oi, "customer_name");
                if (string.IsNullOrEmpty(order.CustomerPhone)) order.CustomerPhone = GetJsonStr(oi, "customer_phone");
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

            order.DateTime = DateTime.TryParse(ts, out DateTime dt) ? dt : DateTime.Now;

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
                    string name = GetJsonStr(item, "product_name");
                    if (string.IsNullOrEmpty(name)) name = GetJsonStr(item, "name");

                    string lineTotal = GetJsonStr(item, "subtotal");
                    if (string.IsNullOrEmpty(lineTotal)) lineTotal = GetJsonStr(item, "line_total");

                    int qty = 1;
                    if (item.TryGetProperty("quantity", out var q) && q.ValueKind == JsonValueKind.Number)
                        qty = q.GetInt32();

                    order.Items.Add(new OrderItem
                    {
                        Name      = name,
                        Quantity  = qty,
                        UnitPrice = GetJsonStr(item, "price"),
                        LineTotal = lineTotal,
                        Notes     = GetJsonStr(item, "notes")
                    });
                }
            }

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

        private static void ShowOrderDetail(Order order)
        {
            string title = $"{LocalizationService.T("orders.detail.dialog_title")} - {order.OrderNumber}";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== {LocalizationService.T("orders.detail.order_header")} {order.OrderNumber} ===");
            sb.AppendLine($"{LocalizationService.T("orders.detail.date")}: {order.DateTime:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"{LocalizationService.T("orders.detail.section")}: {order.Section}");
            sb.AppendLine($"{LocalizationService.T("orders.detail.table")}: {order.TableName}");
            sb.AppendLine($"{LocalizationService.T("orders.detail.waiter")}: {order.WaiterName}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(order.CustomerName))
            {
                sb.AppendLine($"=== {LocalizationService.T("orders.detail.customer")} ===");
                sb.AppendLine($"{order.CustomerName} ({order.CustomerPhone})");
                if (!string.IsNullOrEmpty(order.DeliveryAddress))
                    sb.AppendLine($"{LocalizationService.T("orders.detail.address")}: {order.DeliveryAddress}");
                sb.AppendLine();
            }

            sb.AppendLine($"=== {LocalizationService.T("orders.detail.products")} ===");
            foreach (var item in order.Items)
            {
                sb.AppendLine($"{item.Quantity}x {item.Name} - {item.LineTotal} TL");
                if (item.AddedCustomizations.Count   > 0) sb.AppendLine($"  + {string.Join(", ", item.AddedCustomizations)}");
                if (item.RemovedCustomizations.Count > 0) sb.AppendLine($"  - {string.Join(", ", item.RemovedCustomizations)}");
                if (!string.IsNullOrEmpty(item.Notes))    sb.AppendLine($"  * {item.Notes}");
            }
            sb.AppendLine();
            sb.AppendLine($"{LocalizationService.T("orders.detail.payment_type")}: {order.PaymentMethod}");
            sb.AppendLine($"{LocalizationService.T("orders.detail.total")}: {order.TotalAmount}");
            sb.AppendLine($"{LocalizationService.T("orders.detail.status")}: {order.Status}");
            if (!string.IsNullOrEmpty(order.OrderNote))
            {
                sb.AppendLine();
                sb.AppendLine($"* {order.OrderNote}");
            }

            MessageBox.Show(sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnOrderListChanged()
        {
            if (InvokeRequired)
                Invoke(() => _ = LoadOrders(1));
            else
                _ = LoadOrders(1);
        }
    }
}

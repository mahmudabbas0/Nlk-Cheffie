using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nlk_Cheffie_Print.Core
{
    public class FlatScrollBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _largeChange = 10;
        private int _smallChange = 1;

        private bool _isHovered = false;
        private bool _isDragging = false;
        private int _dragOffset = 0;

        public event EventHandler? Scroll;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Minimum
        {
            get => _minimum;
            set { _minimum = Math.Max(0, value); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(_minimum, value); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set
            {
                int val = Math.Clamp(value, _minimum, Math.Max(_minimum, _maximum - _largeChange + 1));
                if (_value != val)
                {
                    _value = val;
                    Invalidate();
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LargeChange
        {
            get => _largeChange;
            set { _largeChange = Math.Max(1, value); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SmallChange
        {
            get => _smallChange;
            set { _smallChange = Math.Max(1, value); Invalidate(); }
        }

        public FlatScrollBar()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.Width = 10;
            this.BackColor = Color.FromArgb(15, 15, 16); // ColorBackground
        }

        private Rectangle GetThumbRect()
        {
            int trackHeight = Height;
            if (trackHeight <= 0) return Rectangle.Empty;

            int range = _maximum - _minimum + 1;
            if (range <= _largeChange) return Rectangle.Empty;

            int thumbHeight = (int)Math.Max(20f, (float)_largeChange / range * trackHeight);
            int scrollableHeight = trackHeight - thumbHeight;
            float percent = (float)(_value - _minimum) / (_maximum - _minimum - _largeChange + 1);
            int thumbY = (int)(percent * scrollableHeight);

            return new Rectangle(1, thumbY, Width - 2, thumbHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);

            var thumbRect = GetThumbRect();
            if (thumbRect.IsEmpty) return;

            Color thumbColor = _isDragging ? Color.FromArgb(255, 200, 50) 
                             : _isHovered ? ThemeManager.ColorAccent 
                             : Color.FromArgb(160, ThemeManager.ColorAccent);

            using (var brush = new SolidBrush(thumbColor))
            {
                g.FillRectangle(brush, thumbRect);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var thumbRect = GetThumbRect();
                if (thumbRect.Contains(e.Location))
                {
                    _isDragging = true;
                    _dragOffset = e.Y - thumbRect.Y;
                }
                else
                {
                    // Directly center on track click and start drag
                    _isDragging = true;
                    int trackHeight = Height;
                    int range = _maximum - _minimum + 1;
                    int thumbHeight = (int)Math.Max(20f, (float)_largeChange / range * trackHeight);
                    _dragOffset = thumbHeight / 2;

                    int thumbY = e.Y - _dragOffset;
                    int scrollableHeight = trackHeight - thumbHeight;
                    if (scrollableHeight > 0)
                    {
                        thumbY = Math.Clamp(thumbY, 0, scrollableHeight);
                        float percent = (float)thumbY / scrollableHeight;
                        Value = _minimum + (int)Math.Round(percent * (_maximum - _minimum - _largeChange + 1));
                    }
                }
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging)
            {
                int trackHeight = Height;
                int range = _maximum - _minimum + 1;
                int thumbHeight = (int)Math.Max(20f, (float)_largeChange / range * trackHeight);
                int scrollableHeight = trackHeight - thumbHeight;

                if (scrollableHeight > 0)
                {
                    int thumbY = e.Y - _dragOffset;
                    thumbY = Math.Clamp(thumbY, 0, scrollableHeight);
                    float percent = (float)thumbY / scrollableHeight;
                    Value = _minimum + (int)Math.Round(percent * (_maximum - _minimum - _largeChange + 1));
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isDragging = false;
            Invalidate();
            base.OnMouseUp(e);
        }
    }

    public class FlatComboBox : ComboBox
    {
        private Color _borderColor = Color.FromArgb(44, 44, 46); // ColorBorder
        private Color _buttonColor = Color.FromArgb(28, 28, 30);  // ColorFieldBg
        private bool _isHovered = false;
        
        public FlatComboBox()
        {
            SetStyle(ControlStyles.UserPaint | 
                     ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.FromArgb(242, 242, 247);
            
            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = ClientRectangle;

            // 1. Fill background
            using (var bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // 2. Draw dropdown button arrow box (inset to protect border)
            int arrowWidth = 20;
            var arrowRect = new Rectangle(rect.Width - arrowWidth - 1, 1, arrowWidth, rect.Height - 2);
            using (var btnBrush = new SolidBrush(_buttonColor))
            {
                g.FillRectangle(btnBrush, arrowRect);
            }
            
            // 3. Draw active arrow separator line
            Color borderCol = (_isHovered || Focused) ? ThemeManager.ColorAccent : _borderColor;
            using (var separatorPen = new Pen(borderCol, 1))
            {
                g.DrawLine(separatorPen, rect.Width - arrowWidth - 1, 1, rect.Width - arrowWidth - 1, rect.Height - 2);
            }

            // 4. Draw outer border (last, on top)
            using (var borderPen = new Pen(borderCol, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            // 5. Draw triangle arrow
            using (var arrowBrush = new SolidBrush(ForeColor))
            {
                var points = new Point[]
                {
                    new Point(rect.Width - arrowWidth + 6, rect.Height / 2 - 2),
                    new Point(rect.Width - 6, rect.Height / 2 - 2),
                    new Point(rect.Width - arrowWidth / 2, rect.Height / 2 + 3)
                };
                g.FillPolygon(arrowBrush, points);
            }

            // Draw selected text
            string text = (SelectedItem != null ? GetItemText(SelectedItem) : Text) ?? "";
            var textRect = new Rectangle(5, 0, rect.Width - arrowWidth - 8, rect.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.Left;
            TextRenderer.DrawText(g, text, Font, textRect, ForeColor, flags);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? ThemeManager.ColorAccent : Color.FromArgb(28, 28, 30);
            Color fg = isSelected ? Color.Black : Color.FromArgb(242, 242, 247);

            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = GetItemText(Items[e.Index]) ?? "";
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            TextRenderer.DrawText(e.Graphics, text, e.Font ?? Font, e.Bounds, fg, flags);
        }
    }

    public class FlatDateTimePicker : Control
    {
        private DateTime _value = DateTime.Today;
        private string _customFormat = "dd.MM.yyyy";
        private bool _isHovered = false;

        public event EventHandler? ValueChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CustomFormat
        {
            get => _customFormat;
            set { _customFormat = value; Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTimePickerFormat Format { get; set; } = DateTimePickerFormat.Custom;

        public FlatDateTimePicker()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            
            this.BackColor = Color.FromArgb(28, 28, 30); // ColorFieldBg
            this.ForeColor = Color.FromArgb(242, 242, 247); // ColorText
            this.Size = new Size(140, 23);
            this.Cursor = Cursors.Hand;

            this.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = ClientRectangle;

            // 1. Fill background
            using (var bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // 2. Draw dropdown button arrow box (inset to protect border)
            int btnWidth = 20;
            var btnRect = new Rectangle(rect.Width - btnWidth - 1, 1, btnWidth, rect.Height - 2);
            using (var btnBrush = new SolidBrush(Color.FromArgb(28, 28, 30)))
            {
                g.FillRectangle(btnBrush, btnRect);
            }

            // 3. Draw active separator line
            Color borderCol = (_isHovered || Focused) ? ThemeManager.ColorAccent : Color.FromArgb(44, 44, 46);
            using (var separatorPen = new Pen(borderCol, 1))
            {
                g.DrawLine(separatorPen, rect.Width - btnWidth - 1, 1, rect.Width - btnWidth - 1, rect.Height - 2);
            }

            // 4. Draw outer border (last, on top)
            using (var borderPen = new Pen(borderCol, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            // 5. Draw downward triangle arrow
            using (var brush = new SolidBrush(ForeColor))
            {
                var points = new Point[]
                {
                    new Point(rect.Width - btnWidth + 6, rect.Height / 2 - 2),
                    new Point(rect.Width - 6, rect.Height / 2 - 2),
                    new Point(rect.Width - btnWidth / 2, rect.Height / 2 + 3)
                };
                g.FillPolygon(brush, points);
            }

            // 6. Draw formatted date string
            string dateText = _value.ToString(_customFormat);
            var textRect = new Rectangle(5, 0, rect.Width - btnWidth - 8, rect.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            TextRenderer.DrawText(g, dateText, Font, textRect, ForeColor, flags);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowCalendarPopup();
        }

        private void ShowCalendarPopup()
        {
            var popup = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                KeyPreview = true,
                BackColor = Color.FromArgb(44, 44, 46) // matches theme border/accent
            };

            var calendar = new MonthCalendar
            {
                MaxSelectionCount = 1,
                SelectionStart = _value,
                SelectionEnd = _value,
                ShowToday = false,
                ShowTodayCircle = false,
                Location = new Point(12, 8)
            };
            
            // Force native handle creation so SingleMonthSize returns accurate, DPI-scaled measurements
            var forceHandle = calendar.Handle;
            calendar.Size = calendar.SingleMonthSize; // Keep it exactly at native size so text and numbers align perfectly!

            // Create a white panel to seamlessy surround the calendar and act as margins/padding
            var container = new Panel
            {
                BackColor = Color.White,
                Location = new Point(4, 4),
                Size = new Size(calendar.Width + 24, calendar.Height + 16)
            };
            
            container.Controls.Add(calendar);
            popup.Controls.Add(container);
            
            // Set popup size to hold the container plus 4px outer dark border
            popup.Size = new Size(container.Width + 8, container.Height + 8);

            Point screenPt = this.PointToScreen(new Point(0, this.Height));
            var screen = Screen.FromControl(this).WorkingArea;
            if (screenPt.X + popup.Width > screen.Right)
                screenPt.X = screen.Right - popup.Width;
            if (screenPt.Y + popup.Height > screen.Bottom)
                screenPt.Y = this.PointToScreen(Point.Empty).Y - popup.Height;

            popup.Location = screenPt;

            calendar.DateSelected += (s, ev) =>
            {
                Value = ev.Start;
                popup.Close();
            };

            popup.Deactivate += (s, ev) => popup.Close();
            popup.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Escape) popup.Close();
            };

            popup.Show();
            calendar.Focus();
        }
    }

    public class FlatListBox : UserControl
    {
        private InnerListBox _listBox;
        private FlatScrollBar _scrollBar;
        private bool _updatingScroll = false;
        private Point _dragStartPoint = Point.Empty;

        public ListBox.ObjectCollection Items => _listBox.Items;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _listBox.SelectedIndex;
            set { if (value >= -1 && value < Items.Count) _listBox.SelectedIndex = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem
        {
            get => _listBox.SelectedItem;
            set => _listBox.SelectedItem = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TopIndex
        {
            get => _listBox.TopIndex;
            set { if (value >= 0 && value < Items.Count) _listBox.TopIndex = value; }
        }

        public event EventHandler? SelectedIndexChanged;
        public new event EventHandler? DoubleClick;
        public event Action<int, int>? ItemReordered;

        public void ClearSelected() => _listBox.ClearSelected();

        public FlatListBox()
        {
            this.Size = new Size(200, 100);
            this.Padding = new Padding(1);
            this.BackColor = Color.FromArgb(28, 28, 30); // ColorFieldBg

            _scrollBar = new FlatScrollBar
            {
                Width = 8,
                Dock = DockStyle.Right,
                Visible = false,
                Minimum = 0
            };
            _scrollBar.Scroll += ScrollBar_Scroll;

            _listBox = new InnerListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                BackColor = Color.FromArgb(28, 28, 30),
                ForeColor = Color.FromArgb(242, 242, 247),
                AllowDrop = true
            };

            _listBox.SelectedIndexChanged += (s, e) => SelectedIndexChanged?.Invoke(this, e);
            _listBox.DoubleClick += (s, e) => DoubleClick?.Invoke(this, e);
            _listBox.DrawItem += ListBox_DrawItem;
            _listBox.Scrolled += () => UpdateScrollBarValue();

            // Drag and Drop hooks
            _listBox.MouseDown += ListBox_MouseDown;
            _listBox.MouseMove += ListBox_MouseMove;
            _listBox.DragOver += ListBox_DragOver;
            _listBox.DragDrop += ListBox_DragDrop;

            this.Controls.Add(_listBox);
            this.Controls.Add(_scrollBar);

            Application.Idle += (s, e) => UpdateScrollBarValue();
        }

        private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? ThemeManager.ColorAccent : Color.FromArgb(28, 28, 30);
            Color fg = isSelected ? Color.Black : Color.FromArgb(242, 242, 247);

            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            int scrollWidth = _scrollBar.Visible ? _scrollBar.Width : 0;
            var textRect = new Rectangle(e.Bounds.Left + 5, e.Bounds.Top, e.Bounds.Width - 10 - scrollWidth, e.Bounds.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(e.Graphics, Items[e.Index]?.ToString() ?? "", Font, textRect, fg, flags);

            if (!isSelected && e.Index < Items.Count - 1)
            {
                using (var pen = new Pen(Color.FromArgb(44, 44, 46), 1))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - scrollWidth, e.Bounds.Bottom - 1);
                }
            }
        }

        private void ScrollBar_Scroll(object? sender, EventArgs e)
        {
            if (_updatingScroll) return;
            _updatingScroll = true;
            try
            {
                if (_scrollBar.Value >= 0 && _scrollBar.Value < Items.Count)
                {
                    _listBox.TopIndex = _scrollBar.Value;
                }
            }
            finally
            {
                _updatingScroll = false;
            }
        }

        public void RefreshItemsLayout()
        {
            UpdateScrollBarLayout();
        }

        private void UpdateScrollBarLayout()
        {
            int visibleItems = _listBox.Height / _listBox.ItemHeight;
            if (Items.Count > visibleItems)
            {
                _scrollBar.Maximum = Items.Count - 1;
                _scrollBar.LargeChange = visibleItems;
                _scrollBar.Value = _listBox.TopIndex;
                _scrollBar.Visible = true;
            }
            else
            {
                _scrollBar.Visible = false;
            }
        }

        private void UpdateScrollBarValue()
        {
            if (_updatingScroll || !this.IsHandleCreated || this.IsDisposed) return;
            _updatingScroll = true;
            try
            {
                UpdateScrollBarLayout();
                _scrollBar.Value = _listBox.TopIndex;
            }
            catch { }
            finally
            {
                _updatingScroll = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(44, 44, 46), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void ListBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = _listBox.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    _dragStartPoint = e.Location;
                }
                else
                {
                    _dragStartPoint = Point.Empty;
                }
            }
        }

        private void ListBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _dragStartPoint != Point.Empty)
            {
                if (Math.Abs(e.X - _dragStartPoint.X) > SystemInformation.DragSize.Width ||
                    Math.Abs(e.Y - _dragStartPoint.Y) > SystemInformation.DragSize.Height)
                {
                    int index = _listBox.IndexFromPoint(_dragStartPoint);
                    if (index >= 0 && index < Items.Count)
                    {
                        var item = Items[index];
                        _listBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                    _dragStartPoint = Point.Empty;
                }
            }
        }

        private void ListBox_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ListBox_DragDrop(object? sender, DragEventArgs e)
        {
            Point point = _listBox.PointToClient(new Point(e.X, e.Y));
            int index = _listBox.IndexFromPoint(point);
            if (index < 0) index = Items.Count - 1;
            if (index >= Items.Count) index = Items.Count - 1;
            if (index < 0) return;

            if (e.Data == null) return;
            object? draggedItem = null;
            foreach (var format in e.Data.GetFormats())
            {
                var data = e.Data.GetData(format);
                if (data != null)
                {
                    draggedItem = data;
                    break;
                }
            }
            if (draggedItem == null) return;

            int oldIndex = Items.IndexOf(draggedItem);
            if (oldIndex >= 0 && oldIndex != index)
            {
                Items.RemoveAt(oldIndex);
                Items.Insert(index, draggedItem);
                _listBox.SelectedIndex = index;

                ItemReordered?.Invoke(oldIndex, index);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0115 || m.Msg == 0x020A || m.Msg == 0x000F)
            {
                UpdateScrollBarValue();
            }
        }
    }

    internal class InnerListBox : ListBox
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);

        private const int SB_VERT = 1;

        public event Action? Scrolled;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!this.Focused && this.CanFocus)
            {
                this.Focus();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (this.Items.Count > 0)
            {
                int scrollLines = SystemInformation.MouseWheelScrollLines;
                if (scrollLines <= 0) scrollLines = 3;
                int linesToScroll = (e.Delta / 120) * scrollLines;
                if (linesToScroll != 0)
                {
                    int newTop = this.TopIndex - linesToScroll;
                    if (newTop < 0) newTop = 0;
                    if (newTop >= this.Items.Count) newTop = this.Items.Count - 1;
                    this.TopIndex = newTop;
                }
            }
            Scrolled?.Invoke();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (this.IsHandleCreated)
            {
                ShowScrollBar(this.Handle, SB_VERT, false);
            }
            if (m.Msg == 0x0115 || m.Msg == 0x020A || m.Msg == 0x0114) // WM_VSCROLL, WM_MOUSEWHEEL, WM_HSCROLL
            {
                Scrolled?.Invoke();
            }
        }
    }

    public class FlatCheckBox : CheckBox
    {
        private bool _isHovered = false;

        public FlatCheckBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            this.Cursor = Cursors.Hand;
            this.Font = ThemeManager.FontBody;
            this.ForeColor = ThemeManager.ColorText;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Clear background with parent container background or card color
            Color parentBg = Parent?.BackColor ?? ThemeManager.ColorCard;
            g.Clear(parentBg);

            int boxSize = 18;
            int boxY = (Height - boxSize) / 2;
            var boxRect = new Rectangle(0, boxY, boxSize, boxSize);

            if (Checked)
            {
                // Filled Amber/Orange box
                Color boxColor = _isHovered ? ThemeManager.ColorAccentHover : ThemeManager.ColorAccent;
                using (var fillBrush = new SolidBrush(boxColor))
                {
                    g.FillRectangle(fillBrush, boxRect);
                }

                // Sharp black checkmark (✓)
                using (var checkPen = new Pen(Color.FromArgb(15, 15, 16), 2.4f))
                {
                    Point p1 = new Point(boxRect.X + 4, boxRect.Y + 9);
                    Point p2 = new Point(boxRect.X + 7, boxRect.Y + 13);
                    Point p3 = new Point(boxRect.X + 14, boxRect.Y + 5);
                    g.DrawLines(checkPen, new[] { p1, p2, p3 });
                }
            }
            else
            {
                Color borderCol = _isHovered ? ThemeManager.ColorAccent : Color.FromArgb(100, 100, 108);
                using (var bgBrush = new SolidBrush(Color.FromArgb(40, 40, 44)))
                using (var borderPen = new Pen(borderCol, 1.5f))
                {
                    g.FillRectangle(bgBrush, boxRect);
                    g.DrawRectangle(borderPen, boxRect);
                }
            }

            // Draw label text neatly aligned to the right of check box
            if (!string.IsNullOrEmpty(Text))
            {
                var textRect = new Rectangle(boxSize + 8, 0, Width - (boxSize + 8), Height);
                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }
    }
}

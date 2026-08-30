using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers
{
    public enum HeaderButton
    {
        None,
        Close,
        Minimize,
        Help
    }

    /// <summary>
    /// Self-contained HWorld window header.
    /// Draws the title bar, handles dragging and optional window buttons.
    /// </summary>
    [DefaultEvent("PerformOnClose")]
    public sealed class Header : UserControl
    {
        private const int ButtonWidth = 44;

        private bool _dragging;
        private Point _lastMousePosition;
        private HeaderButton _hoveredButton;
        private HeaderButton _pressedButton;

        private string _title = "HWorld";
        private string _subtitle = string.Empty;
        private Image _headerIcon;
        private Image _cachedIcon;
        private int _iconSize = 22;
        private int _iconMargin = 12;
        private int _textMargin = 8;
        private int _headerHeight = 54;

        private Color _background1 = Color.FromArgb(31, 24, 69);
        private Color _background2 = Color.FromArgb(88, 39, 126);
        private Color _foreground = Color.FromArgb(246, 244, 255);
        private Color _subtitleForeground = Color.FromArgb(214, 205, 235);
        private Color _buttonHover = Color.FromArgb(70, 255, 255, 255);
        private Color _buttonPressed = Color.FromArgb(90, 255, 255, 255);
        private Color _closeHover = Color.FromArgb(218, 70, 102);

        public event EventHandler PerformOnClose;
        public event EventHandler PerformOnHelp;
        public event EventHandler PerformOnMinimize;

        public Header()
        {
            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            Height = _headerHeight;
            MinimumSize = new Size(0, _headerHeight);
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
            BackColor = Color.Transparent;
            this.AllowHelp = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _cachedIcon != null)
            {
                _cachedIcon.Dispose();
                _cachedIcon = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            var button = HitTest(e.Location);
            if (button == HeaderButton.None && AllowMove)
            {
                _dragging = true;
                _lastMousePosition = Cursor.Position;
                Capture = true;
                return;
            }

            _pressedButton = button;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var button = HitTest(e.Location);
            if (button != _hoveredButton)
            {
                _hoveredButton = button;
                Invalidate();
            }

            if (!_dragging) return;

            var target = DragTarget ?? FindForm();
            if (target == null) return;

            var current = Cursor.Position;
            var dx = current.X - _lastMousePosition.X;
            var dy = current.Y - _lastMousePosition.Y;

            if (dx != 0 || dy != 0)
                target.Location = new Point(target.Location.X + dx, target.Location.Y + dy);

            _lastMousePosition = current;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            if (_dragging)
            {
                _dragging = false;
                Capture = false;
            }
            else
            {
                var button = HitTest(e.Location);
                if (button == _pressedButton)
                    ExecuteButton(button);
            }

            _pressedButton = HeaderButton.None;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredButton != HeaderButton.None)
            {
                _hoveredButton = HeaderButton.None;
                Invalidate();
            }
        }

        private HeaderButton HitTest(Point point)
        {
            var width = Width;
            if (AllowClose && new Rectangle(width - ButtonWidth, 0, ButtonWidth, Height).Contains(point))
                return HeaderButton.Close;

            var next = width - ButtonWidth * 2;
            if (AllowMinimize && new Rectangle(next, 0, ButtonWidth, Height).Contains(point))
                return HeaderButton.Minimize;

            var helpX = width - ButtonWidth * 3;
            if (AllowHelp && new Rectangle(helpX, 0, ButtonWidth, Height).Contains(point))
                return HeaderButton.Help;

            return HeaderButton.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var brush = new LinearGradientBrush(ClientRectangle, _background1, _background2, 90f))
                g.FillRectangle(brush, ClientRectangle);

            DrawContent(g);
           // DrawButton(g, HeaderButton.Help, AllowHelp, "?");
            DrawButton(g, HeaderButton.Minimize, AllowMinimize, "—");
            DrawButton(g, HeaderButton.Close, AllowClose, "×");
        }

        private void DrawContent(Graphics g)
        {
            var textLeft = _iconMargin;
            if (_cachedIcon != null)
            {
                var iconRect = new Rectangle(_iconMargin, Math.Max(0, (Height - _iconSize) / 2), _iconSize, _iconSize);
                g.DrawImage(_cachedIcon, iconRect);
                textLeft += _iconSize + _textMargin;
            }

            var buttonSpace =
                (AllowClose ? ButtonWidth : 0) +
                (AllowMinimize ? ButtonWidth : 0) +
                (AllowHelp ? ButtonWidth : 0);

            var textWidth = Math.Max(0, Width - textLeft - buttonSpace - _textMargin);
            var titleRect = new Rectangle(textLeft, 4, textWidth, Math.Max(18, Height / 2));

            if (string.IsNullOrWhiteSpace(_subtitle))
            {
                TextRenderer.DrawText(g, _title, Font, new Rectangle(textLeft, 0, textWidth, Height), _foreground,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                return;
            }

            using (var subtitleFont = new Font("Segoe UI", 7.8f))
            {
                TextRenderer.DrawText(g, _title, Font, titleRect, _foreground,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                var subtitleRect = new Rectangle(textLeft, Height / 2 - 1, textWidth, Height / 2);
                TextRenderer.DrawText(g, _subtitle, subtitleFont, subtitleRect, _subtitleForeground,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private void DrawButton(Graphics g, HeaderButton button, bool visible, string glyph)
        {
            if (!visible) return;

            var x = Width - ButtonWidth;
            if (button == HeaderButton.Minimize) x -= ButtonWidth;
            if (button == HeaderButton.Help) x -= ButtonWidth;

            var rect = new Rectangle(x, 0, ButtonWidth, Height);
            var hovered = _hoveredButton == button;
            var pressed = _pressedButton == button && hovered;

            if (hovered || pressed)
            {
                var color = button == HeaderButton.Close
                    ? _closeHover
                    : (pressed ? _buttonPressed : _buttonHover);
                using (var brush = new SolidBrush(color))
                    g.FillRectangle(brush, rect);
            }

            using (var font = new Font("Segoe UI Symbol", button == HeaderButton.Help ? 10f : 13f,
                button == HeaderButton.Help ? FontStyle.Bold : FontStyle.Regular))
            {
                TextRenderer.DrawText(g, glyph, font, rect, _foreground,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        private void ExecuteButton(HeaderButton button)
        {
            var form = FindForm();
            switch (button)
            {
                case HeaderButton.Close:
                    PerformOnClose?.Invoke(this, EventArgs.Empty);
                    if (form != null) form.Close();
                    break;
                case HeaderButton.Minimize:
                    PerformOnMinimize?.Invoke(this, EventArgs.Empty);
                    if (form != null) form.WindowState = FormWindowState.Minimized;
                    break;
                case HeaderButton.Help:
                    PerformOnHelp?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void UpdateCachedIcon()
        {
            if (_cachedIcon != null)
            {
                _cachedIcon.Dispose();
                _cachedIcon = null;
            }

            if (_headerIcon != null)
                _cachedIcon = new Bitmap(_headerIcon, _iconSize, _iconSize);
        }

        [Category("HHeader")]
        public string Title
        {
            get { return _title; }
            set { _title = value ?? string.Empty; Text = _title; Invalidate(); }
        }

        [Category("HHeader")]
        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value ?? string.Empty; Invalidate(); }
        }

        [Category("HHeader")]
        public bool AllowMove { get; set; } = true;

        [Category("HHeader Buttons")]
        public bool AllowClose { get; set; } = true;

        [Category("HHeader Buttons")]
        public bool AllowMinimize { get; set; }

        [Category("HHeader Buttons")]
        public bool AllowHelp { get; set; }

        [Category("HHeader")]
        public Control DragTarget { get; set; }

        [Category("HHeader")]
        public int HeaderHeight
        {
            get { return _headerHeight; }
            set
            {
                _headerHeight = Math.Max(24, value);
                MinimumSize = new Size(0, _headerHeight);
                Height = _headerHeight;
                Invalidate();
            }
        }

        [Category("HHeader Image")]
        public Image HeaderIcon
        {
            get { return _headerIcon; }
            set { _headerIcon = value; UpdateCachedIcon(); Invalidate(); }
        }

        [Category("HHeader Image")]
        public int IconSize
        {
            get { return _iconSize; }
            set { _iconSize = Math.Max(1, value); UpdateCachedIcon(); Invalidate(); }
        }

        [Category("HHeader Image")]
        public int IconMargin
        {
            get { return _iconMargin; }
            set { _iconMargin = Math.Max(0, value); Invalidate(); }
        }

        [Category("HHeader")]
        public int TextMargin
        {
            get { return _textMargin; }
            set { _textMargin = Math.Max(0, value); Invalidate(); }
        }

        [Category("HHeader Color")]
        public Color BackGroundColor1 { get { return _background1; } set { _background1 = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color BackGroundColor2 { get { return _background2; } set { _background2 = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ForeColor1 { get { return _foreground; } set { _foreground = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color SubtitleColor { get { return _subtitleForeground; } set { _subtitleForeground = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ButtonHoverColor { get { return _buttonHover; } set { _buttonHover = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ButtonPressedColor { get { return _buttonPressed; } set { _buttonPressed = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color CloseHoverColor { get { return _closeHover; } set { _closeHover = value; Invalidate(); } }
    }
}

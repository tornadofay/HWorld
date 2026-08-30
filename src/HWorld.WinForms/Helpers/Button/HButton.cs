using HWorld.WinForms.Helpers.Button.HButtonCore;
using HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape;
using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using HWorld.ImageCore;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button
{
    [DefaultEvent("Click")]
    public partial class HButton : UserControl
    {
        #region Win32
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSELEAVE = 0x02A3;
        private const int WM_DPICHANGED = 0x02E0;
        private const uint TME_LEAVE = 0x00000002;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        [StructLayout(LayoutKind.Sequential)]
        private struct TRACKMOUSEEVENT { public int cbSize; public uint dwFlags; public IntPtr hwndTrack; public uint dwHoverTime; }
        #endregion

        private readonly HButtonStateMachine _state = new HButtonStateMachine();
        private readonly HButtonLayoutEngine _layoutEngine = new HButtonLayoutEngine();
        private IRendererResourceProvider _resourceProvider = RendererResourceManager.Global;
        private IHButtonRenderer _renderer;

        private HButtonLayoutResult _cachedLayout;
        private bool _isTracking;
        private uint _geometryVersion;
        private uint _lastLayoutVersion;
        private uint _lastRendererLayoutVersion = uint.MaxValue;
        private float _scaleFactor = 1f;

        private int _edge = 10;
        private bool _roundButton = true;
        private Image _image;
        private int _imageWidth = 32, _imageHeight = 32, _imageMargin = 5, _textMargin = 5;
        private ContentAlignment _imageAlign = ContentAlignment.MiddleLeft;
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        private AlignmentType _alignmentMode = AlignmentType.ContentAlignment;

        private Color _leaveBg1 = Color.FromArgb(41, 41, 41), _leaveBg2 = Color.Black, _leaveFore = Color.FromArgb(194, 219, 249), _leaveBorder = Color.FromArgb(50, 50, 50);
        private Color _enterBg1 = Color.FromArgb(105, 95, 87), _enterBg2 = Color.Black, _enterFore = Color.Gold, _enterBorder = Color.FromArgb(50, 50, 50);
        private Color _downBg1 = Color.FromArgb(39, 55, 52), _downBg2 = Color.Black, _downFore = Color.FromArgb(67, 67, 67), _downBorder = Color.FromArgb(50, 50, 50);

        public HButton()
        {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.StandardClick |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor,
                true);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;

            _state.StateChanged += OnStateChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _renderer?.Dispose();

                // Clean up Region
                var oldRegion = Region;
                Region = null;
                oldRegion?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnThemeChanged(object sender, EventArgs e) => InvalidateVisual();
        private void OnStateChanged(object s, EventArgs e) => InvalidateVisual();
        private void InvalidateGeometry() { unchecked { _geometryVersion++; } RequestRepaint(); }
        private void InvalidateVisual() => RequestRepaint();

        private int _updateCount;
        private bool _pendingInvalidate;
        public IDisposable SuspendUpdates() { BeginUpdate(); return new Suspension(this); }
        public void BeginUpdate() => _updateCount++;
        public void EndUpdate() { if (_updateCount > 0 && --_updateCount == 0 && _pendingInvalidate) { _pendingInvalidate = false; Invalidate(); } }
        private void RequestRepaint() { if (_updateCount > 0) _pendingInvalidate = true; else Invalidate(); }
        private sealed class Suspension : IDisposable { private readonly HButton _b; public Suspension(HButton b) => _b = b; public void Dispose() => _b.EndUpdate(); }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var ctx = new HButtonRenderContext(
                new HButtonState(_state.State, ShowFocusCues),
                new HButtonGeometry(ClientRectangle, _imageWidth, _imageHeight, _imageMargin, _textMargin, _imageAlign, _textAlign, _alignmentMode,
                    RightToLeft, _scaleFactor, _imageSizeMode),
                new HButtonContent(_image, Text, Font),
                new HButtonAppearance(_roundButton, _roundStyle, _edge, new HButtonCustomColors(_leaveBg1, _leaveBg2, _leaveFore, _leaveBorder, _enterBg1, _enterBg2, _enterFore, _enterBorder, _downBg1, _downBg2, _downFore, _downBorder)));

            if (_lastLayoutVersion != _geometryVersion || _lastRendererLayoutVersion != Renderer.LayoutVersion)
            {
                _cachedLayout = _layoutEngine.Layout(in ctx.Geometry, in ctx.Content);
                _lastLayoutVersion = _geometryVersion;
                _lastRendererLayoutVersion = Renderer.LayoutVersion;
            }
            Renderer.Draw(e.Graphics, in ctx, in _cachedLayout);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // When the form is restored from minimize, force a complete repaint
            if (Visible)
            {
                UpdateRegion();      // Re-apply rounded corners region
                InvalidateGeometry(); // Force layout recalculation
                Invalidate(true);     // Force full repaint including children
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // If the button is moved to a new parent, rebuild everything
            if (Visible)
            {
                UpdateRegion();
                InvalidateGeometry();
            }
        }

        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);
        protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); if (e.KeyCode == Keys.Space) _state.KeyDownSpace(); else if (e.KeyCode == Keys.Enter) OnClick(EventArgs.Empty); }
        protected override void OnKeyUp(KeyEventArgs e) { base.OnKeyUp(e); if (e.KeyCode == Keys.Space) { _state.KeyUpSpace(); OnClick(EventArgs.Empty); } }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) _state.MouseDown(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); if (e.Button == MouseButtons.Left) _state.MouseUp(ClientRectangle.Contains(PointToClient(Cursor.Position))); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); _state.SetEnabled(Enabled); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); _state.FocusGained(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); _state.FocusLost(); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); UpdateRegion(); InvalidateGeometry(); }
 


        private const int WM_SIZE = 0x0005;
        private const int SIZE_RESTORED = 0;
        private const int SIZE_MINIMIZED = 1;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Catch the form restore message and force repaint
            if (m.Msg == WM_SIZE)
            {
                int wParam = m.WParam.ToInt32();
                if (wParam == SIZE_RESTORED)
                {
                    // Small delay to ensure the form is fully restored
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed && Visible)
                        {
                            UpdateRegion();
                            Invalidate(true);
                        }
                    }));
                }
            }

            if (m.Msg == WM_MOUSEMOVE) { if (!_isTracking) { var tme = new TRACKMOUSEEVENT { cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT)), dwFlags = TME_LEAVE, hwndTrack = Handle }; TrackMouseEvent(ref tme); _isTracking = true; _state.MouseEnter(); } }
            else if (m.Msg == WM_MOUSELEAVE) { _isTracking = false; _state.MouseLeave(); }
           
        }

        public IHButtonRenderer Renderer
        {
            get => _renderer ?? (_renderer = new HButtonClassicRenderer(_resourceProvider.GetOrCreate<ClassicRendererResources>()));
            set { var next = value ?? new HButtonClassicRenderer(_resourceProvider.GetOrCreate<ClassicRendererResources>()); if (ReferenceEquals(_renderer, next)) return; _renderer?.Dispose(); _renderer = next; InvalidateGeometry(); }
        }

        [Category("HControls © Round")] public bool RoundButton { get => _roundButton; set { if (_roundButton != value) { _roundButton = value; UpdateRegion(); InvalidateVisual(); } } }
        [Category("HControls © Border")] public int Edge { get => _edge; set { if (_edge != value) { _edge = value == 0 ? 1 : value; UpdateRegion(); InvalidateVisual(); } } }
        [Category("HControls © Image"), Localizable(true)] public Image Image { get => _image; set { if (_image != value) { _image = value; InvalidateGeometry(); } } }
        [Category("HControls © Image")] public int ImageWidth { get => _imageWidth; set { if (_imageWidth != value) { _imageWidth = value; InvalidateGeometry(); } } }
        [Category("HControls © Image")] public int ImageHeight { get => _imageHeight; set { if (_imageHeight != value) { _imageHeight = value; InvalidateGeometry(); } } }
        [Category("HControls © Image")] public int ImageMargin { get => _imageMargin; set { if (_imageMargin != value) { _imageMargin = value; InvalidateGeometry(); } } }
        [Category("HControls © Text")] public int TextMargin { get => _textMargin; set { if (_textMargin != value) { _textMargin = value; InvalidateGeometry(); } } }
        [Category("HControls © Alignment"), Localizable(true)] public ContentAlignment ImageAlign { get => _imageAlign; set { if (_imageAlign != value) { _imageAlign = value; InvalidateGeometry(); } } }
        [Category("HControls © Alignment"), Localizable(true)] public ContentAlignment TextAlign { get => _textAlign; set { if (_textAlign != value) { _textAlign = value; InvalidateGeometry(); } } }
        [Category("HControls © Alignment"), Localizable(true)] public AlignmentType AlignmentMode { get => _alignmentMode; set { if (_alignmentMode != value) { _alignmentMode = value; InvalidateGeometry(); } } }

        public override RightToLeft RightToLeft { get => base.RightToLeft; set { if (base.RightToLeft != value) { base.RightToLeft = value; InvalidateGeometry(); } } }
        [Category("HControls © Text"), Browsable(true)]
        public override string Text { get => base.Text; set { if (base.Text != value) { base.Text = value; InvalidateGeometry(); } } }
        public override Font Font { get => base.Font; set { if (base.Font != value) { base.Font = value; InvalidateGeometry(); } } }

        [Category("HControls © Color Leave")] public Color ButtonLeaveBackGroundColor1 { get => _leaveBg1; set { if (_leaveBg1 != value) { _leaveBg1 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Leave")] public Color ButtonLeaveBackGroundColor2 { get => _leaveBg2; set { if (_leaveBg2 != value) { _leaveBg2 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Leave")] public Color ButtonLeaveForeColor { get => _leaveFore; set { if (_leaveFore != value) { _leaveFore = value; InvalidateVisual(); } } }
        [Category("HControls © Color Leave")] public Color ButtonLeaveBorderColor { get => _leaveBorder; set { if (_leaveBorder != value) { _leaveBorder = value; InvalidateVisual(); } } }

        [Category("HControls © Color Enter")] public Color ButtonEnterBackGroundColor1 { get => _enterBg1; set { if (_enterBg1 != value) { _enterBg1 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Enter")] public Color ButtonEnterBackGroundColor2 { get => _enterBg2; set { if (_enterBg2 != value) { _enterBg2 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Enter")] public Color ButtonEnterForeColor { get => _enterFore; set { if (_enterFore != value) { _enterFore = value; InvalidateVisual(); } } }
        [Category("HControls © Color Enter")] public Color ButtonEnterBorderColor { get => _enterBorder; set { if (_enterBorder != value) { _enterBorder = value; InvalidateVisual(); } } }

        [Category("HControls © Color Down")] public Color ButtonDownBackGroundColor1 { get => _downBg1; set { if (_downBg1 != value) { _downBg1 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Down")] public Color ButtonDownBackGroundColor2 { get => _downBg2; set { if (_downBg2 != value) { _downBg2 = value; InvalidateVisual(); } } }
        [Category("HControls © Color Down")] public Color ButtonDownForeColor { get => _downFore; set { if (_downFore != value) { _downFore = value; InvalidateVisual(); } } }
        [Category("HControls © Color Down")] public Color ButtonDownBorderColor { get => _downBorder; set { if (_downBorder != value) { _downBorder = value; InvalidateVisual(); } } }

        // Original BackGroundColor1 / ForeColor / BorderColor properties preserved
        [Category("HControls © Color"), Description("BackGround Color1.")] public Color BackGroundColor1 { get => _leaveBg1; set { if (_leaveBg1 != value) { _leaveBg1 = value; InvalidateVisual(); } } }
        [Category("HControls © Color"), Description("BackGround Color2.")] public Color BackGroundColor2 { get => _leaveBg2; set { if (_leaveBg2 != value) { _leaveBg2 = value; InvalidateVisual(); } } }
        [Category("HControls © Color"), Description("Fore Color.")] public override Color ForeColor { get => base.ForeColor; set { if (base.ForeColor != value) { base.ForeColor = value; _leaveFore = value; InvalidateVisual(); } } }
        [Category("HControls © Color"), Description("Button Border Color.")] public Color BorderColor { get => _leaveBorder; set { if (_leaveBorder != value) { _leaveBorder = value; InvalidateVisual(); } } }



        private bool _roundStyle = false;
        [Category("HControls © Round"), Description("Use Radial/Elliptical Gradient instead of Linear.")]
        public bool RoundStyle
        {
            get => _roundStyle;
            set { if (_roundStyle != value) { _roundStyle = value; InvalidateVisual(); } }
        }

        private ImageSizeMode _imageSizeMode = ImageSizeMode.Normal;

        [Category("HControls © Image"), Description("How the image is sized within the button.")]
        public ImageSizeMode ImageSizeMode
        {
            get => _imageSizeMode;
            set { if (_imageSizeMode != value) { _imageSizeMode = value; InvalidateGeometry(); } }
        }

        private void UpdateRegion()
        {
            if (!IsHandleCreated) return;

            if (_roundButton && Width > 0 && Height > 0)
            {
                int scaledRadius = (int)(_edge * _scaleFactor);
                using (var path = ShapeFactory.Create(ClientRectangle, scaledRadius, true))
                {
                    var newRegion = new Region(path);
                    var oldRegion = Region;
                    Region = newRegion;
                    oldRegion?.Dispose();
                }
            }
            else
            {
                var oldRegion = Region;
                Region = null;
                oldRegion?.Dispose();
            }
        }
    }
}

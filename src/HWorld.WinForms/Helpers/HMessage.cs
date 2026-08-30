using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers
{
    #region Public API

    /// <summary>
    /// Semantic type of a message dialog.
    /// </summary>
    public enum HMessageType
    {
        Information,
        Success,
        Warning,
        Error,
        Question,
        Delete,   // <-- ADDED
        Exit,     // <-- ADDED
        Hide      // <-- ADDED
    }

    /// <summary>
    /// Buttons displayed by the message dialog.
    /// </summary>
    public enum HMessageButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// Color theme used by the dialog.
    /// </summary>
    public enum HMessageThemeMode
    {
        /// <summary>Follow the Windows app theme (detected once per process).</summary>
        System,
        Light,
        Dark
    }

    /// <summary>
    /// UI language used for the built-in button/link captions.
    /// </summary>
    public enum HMessageLanguage
    {
        English,
        Arabic
    }

    #endregion

    #region Theme Palette

    /// <summary>
    /// Color tokens used to render the dialog. Use <see cref="HMessagePalette.Light"/>,
    /// <see cref="HMessagePalette.Dark"/> or derive your own brand theme.
    /// </summary>
    public sealed class HMessagePalette
    {
        public Color Surface { get; private set; }
        public Color SurfaceSecondary { get; private set; }
        public Color Border { get; private set; }
        public Color TextPrimary { get; private set; }
        public Color TextSecondary { get; private set; }
        public Color DetailsBackground { get; private set; }
        public Color DetailsText { get; private set; }

        public Color Information { get; private set; }
        public Color Success { get; private set; }
        public Color Warning { get; private set; }
        public Color Error { get; private set; }
        public Color Question { get; private set; }
        public Color Delete { get; private set; }
        public Color Exit { get; private set; }
        public Color Hide { get; private set; }

        public Color PrimaryButton { get; private set; }
        public Color PrimaryButtonHover { get; private set; }
        public Color PrimaryButtonPressed { get; private set; }
        public Color PrimaryButtonText { get; private set; }

        public Color SecondaryButton { get; private set; }
        public Color SecondaryButtonHover { get; private set; }
        public Color SecondaryButtonPressed { get; private set; }
        public Color SecondaryButtonText { get; private set; }
        public Color SecondaryButtonBorder { get; private set; }

        public Color Link { get; private set; }
        public Color LinkHover { get; private set; }

        public Color DisabledBackground { get; private set; }
        public Color DisabledText { get; private set; }

        public static HMessagePalette CreateLight()
        {
            return new HMessagePalette
            {
                Surface = Color.FromArgb(255, 255, 255),
                SurfaceSecondary = Color.FromArgb(245, 246, 248),
                Border = Color.FromArgb(216, 220, 226),
                TextPrimary = Color.FromArgb(27, 29, 33),
                TextSecondary = Color.FromArgb(90, 95, 104),
                DetailsBackground = Color.FromArgb(245, 246, 248),
                DetailsText = Color.FromArgb(48, 51, 57),

                Information = Color.FromArgb(37, 118, 208),
                Success = Color.FromArgb(22, 158, 87),
                Warning = Color.FromArgb(199, 136, 6),
                Error = Color.FromArgb(207, 62, 62),
                Question = Color.FromArgb(94, 99, 211),
                Delete = Color.FromArgb(207, 62, 62),   // Red (like Error)
                Exit = Color.FromArgb(199, 136, 6),    // Orange (like Warning)
                Hide = Color.FromArgb(90, 95, 104),    // Muted gray

                PrimaryButton = Color.FromArgb(77, 87, 201),
                PrimaryButtonHover = Color.FromArgb(66, 76, 186),
                PrimaryButtonPressed = Color.FromArgb(55, 64, 166),
                PrimaryButtonText = Color.White,

                SecondaryButton = Color.FromArgb(243, 244, 247),
                SecondaryButtonHover = Color.FromArgb(233, 236, 240),
                SecondaryButtonPressed = Color.FromArgb(222, 226, 232),
                SecondaryButtonText = Color.FromArgb(40, 43, 49),
                SecondaryButtonBorder = Color.FromArgb(212, 216, 223),

                Link = Color.FromArgb(37, 118, 208),
                LinkHover = Color.FromArgb(22, 92, 172),

                DisabledBackground = Color.FromArgb(233, 235, 238),
                DisabledText = Color.FromArgb(150, 154, 161)
            };
        }

        public static HMessagePalette CreateDark()
        {
            return new HMessagePalette
            {
                Surface = Color.FromArgb(34, 35, 40),
                SurfaceSecondary = Color.FromArgb(42, 44, 50),
                Border = Color.FromArgb(66, 69, 77),
                TextPrimary = Color.FromArgb(240, 242, 246),
                TextSecondary = Color.FromArgb(166, 172, 182),
                DetailsBackground = Color.FromArgb(27, 28, 32),
                DetailsText = Color.FromArgb(206, 210, 217),

                Information = Color.FromArgb(92, 152, 236),
                Success = Color.FromArgb(74, 190, 122),
                Warning = Color.FromArgb(232, 172, 62),
                Error = Color.FromArgb(236, 106, 106),
                Question = Color.FromArgb(142, 147, 240),
                Delete = Color.FromArgb(236, 106, 106),
                Exit = Color.FromArgb(232, 172, 62),
                Hide = Color.FromArgb(166, 172, 182),

                PrimaryButton = Color.FromArgb(106, 116, 232),
                PrimaryButtonHover = Color.FromArgb(122, 131, 240),
                PrimaryButtonPressed = Color.FromArgb(90, 100, 206),
                PrimaryButtonText = Color.White,

                SecondaryButton = Color.FromArgb(54, 57, 64),
                SecondaryButtonHover = Color.FromArgb(65, 68, 76),
                SecondaryButtonPressed = Color.FromArgb(74, 78, 87),
                SecondaryButtonText = Color.FromArgb(226, 229, 235),
                SecondaryButtonBorder = Color.FromArgb(78, 82, 91),

                Link = Color.FromArgb(128, 168, 242),
                LinkHover = Color.FromArgb(158, 190, 248),

                DisabledBackground = Color.FromArgb(56, 58, 64),
                DisabledText = Color.FromArgb(120, 124, 132)
            };
        }

        /// <summary>
        /// Palette derived from the active high-contrast system colors.
        /// </summary>
        public static HMessagePalette CreateHighContrast()
        {
            return new HMessagePalette
            {
                Surface = SystemColors.Window,
                SurfaceSecondary = SystemColors.Control,
                Border = SystemColors.WindowFrame,
                TextPrimary = SystemColors.WindowText,
                TextSecondary = SystemColors.WindowText,
                DetailsBackground = SystemColors.Window,
                DetailsText = SystemColors.WindowText,

                Information = SystemColors.Highlight,
                Success = SystemColors.Highlight,
                Warning = SystemColors.Highlight,
                Error = SystemColors.Highlight,
                Question = SystemColors.Highlight,
                Delete = SystemColors.Highlight,
                Exit = SystemColors.Highlight,
                Hide = SystemColors.Highlight,

                PrimaryButton = SystemColors.Highlight,
                PrimaryButtonHover = SystemColors.Highlight,
                PrimaryButtonPressed = SystemColors.Highlight,
                PrimaryButtonText = SystemColors.HighlightText,

                SecondaryButton = SystemColors.ButtonFace,
                SecondaryButtonHover = SystemColors.ButtonFace,
                SecondaryButtonPressed = SystemColors.ButtonFace,
                SecondaryButtonText = SystemColors.ControlText,
                SecondaryButtonBorder = SystemColors.ButtonShadow,

                Link = SystemColors.HotTrack,
                LinkHover = SystemColors.HotTrack,

                DisabledBackground = SystemColors.ButtonFace,
                DisabledText = SystemColors.GrayText
            };
        }
    }

    #endregion

    #region Dialog Options

    /// <summary>
    /// Full set of options for <see cref="HMessage.Show(IWin32Window, HMessageOptions)"/>.
    /// Every classic overload maps onto a small subset of these options.
    /// </summary>
    public sealed class HMessageOptions
    {
        public HMessageOptions()
        {
            Type = HMessageType.Information;
            Buttons = HMessageButtons.Ok;
            DefaultResult = DialogResult.None;
            PlaySound = true;
            AllowCopyShortcut = true;
            Theme = HMessageThemeMode.System;
        }

        /// <summary>Body text of the dialog.</summary>
        public string Message { get; set; }

        /// <summary>Title line rendered above the message.</summary>
        public string Caption { get; set; }

        /// <summary>Semantic type (drives icon, accent color, sound).</summary>
        public HMessageType Type { get; set; }

        /// <summary>Button set.</summary>
        public HMessageButtons Buttons { get; set; }

        /// <summary>Optional technical details behind the "Show details" link.</summary>
        public string Details { get; set; }

        /// <summary>Show the details panel expanded instead of collapsed.</summary>
        public bool DetailsExpanded { get; set; }

        /// <summary>Overrides the localized caption of the primary button (null = default).</summary>
        public string PrimaryButtonText { get; set; }

        /// <summary>Overrides the localized caption of the secondary button (null = default).</summary>
        public string SecondaryButtonText { get; set; }

        /// <summary>Overrides the localized caption of the tertiary button (null = default).</summary>
        public string TertiaryButtonText { get; set; }

        /// <summary>
        /// The result activated by Enter and by the auto-close timeout.
        /// <see cref="DialogResult.None"/> (default) picks the conventional choice
        /// (OK for Ok/OkCancel, Yes for YesNo/YesNoCancel).
        /// </summary>
        public DialogResult DefaultResult { get; set; }

        /// <summary>Play the system sound associated with <see cref="Type"/>. Default true.</summary>
        public bool PlaySound { get; set; }

        /// <summary>Allow Ctrl+C to copy a formatted report of the dialog. Default true.</summary>
        public bool AllowCopyShortcut { get; set; }

        /// <summary>
        /// If greater than zero, the dialog closes itself after this many seconds,
        /// returning <see cref="DefaultResult"/>. The default button shows the countdown.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Optional verification check-box text, e.g. "Don't show again".
        /// The caller provides an already-localized string; null hides the check-box.
        /// </summary>
        public string VerificationText { get; set; }

        /// <summary>
        /// In: initial state of the verification check-box.
        /// Out: state when the dialog closed (read it back after Show returns).
        /// </summary>
        public bool VerificationChecked { get; set; }

        /// <summary>Per-call theme override. Default: <see cref="HMessageThemeMode.System"/>.</summary>
        public HMessageThemeMode Theme { get; set; }
    }

    #endregion

    #region Static Dialog API

    /// <summary>
    /// A lightweight, modern WinForms message dialog.
    /// </summary>
    public static class HMessage
    {
        #region Global Settings

        private static HMessageThemeMode _themeMode = HMessageThemeMode.System;
        private static bool _animationsEnabled = true;
        private static int _detectedTheme = -1; // -1 unknown, 0 light, 1 dark

        private static HMessagePalette _light;
        private static HMessagePalette _dark;
        private static HMessagePalette _highContrast;

        /// <summary>
        /// Resolves the UI language for built-in captions. Wire it to your
        /// application's language service once at startup. Default: English.
        /// </summary>
        public static Func<HMessageLanguage> LanguageProvider { get; set; }

        /// <summary>
        /// Global theme. Default <see cref="HMessageThemeMode.System"/> follows Windows.
        /// </summary>
        public static HMessageThemeMode ThemeMode
        {
            get { return _themeMode; }
            set { _themeMode = value; }
        }

        /// <summary>
        /// Master switch for the fade animation. The OS animation setting and
        /// high-contrast mode are always respected on top of this flag.
        /// </summary>
        public static bool AnimationsEnabled
        {
            get { return _animationsEnabled; }
            set { _animationsEnabled = value; }
        }

        internal static bool IsArabic
        {
            get
            {
                Func<HMessageLanguage> provider = LanguageProvider;
                if (provider != null)
                {
                    return provider() == HMessageLanguage.Arabic;
                }

                // FALLBACK: Automatically detect from the current thread's UI culture.
                // This ensures the dialog works correctly even if the host app only 
                // changes Thread.CurrentThread.CurrentUICulture and forgets to set LanguageProvider.
                string cultureName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                return cultureName.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            }
        }

        internal static string GetText(string english, string arabic)
        {
            return IsArabic ? arabic : english;
        }

        internal static bool IsAnimationEnabled
        {
            get
            {
                if (!_animationsEnabled || SystemInformation.HighContrast)
                {
                    return false;
                }

                bool enabled = true;

                try
                {
                    NativeMethods.SystemParametersInfo(
                        NativeMethods.SPI_GETCLIENTAREAANIMATION,
                        0,
                        ref enabled,
                        0);
                }
                catch
                {
                    // Animation setting unavailable -> keep animating.
                }

                return enabled;
            }
        }

        internal static HMessagePalette ResolvePalette(HMessageThemeMode mode)
        {
            if (SystemInformation.HighContrast)
            {
                if (_highContrast == null)
                {
                    _highContrast = HMessagePalette.CreateHighContrast();
                }

                return _highContrast;
            }

            if (mode == HMessageThemeMode.System)
            {
                mode = DetectSystemTheme();
            }

            if (mode == HMessageThemeMode.Dark)
            {
                if (_dark == null)
                {
                    _dark = HMessagePalette.CreateDark();
                }

                return _dark;
            }

            if (_light == null)
            {
                _light = HMessagePalette.CreateLight();
            }

            return _light;
        }

        private static HMessageThemeMode DetectSystemTheme()
        {
            if (_detectedTheme < 0)
            {
                _detectedTheme = 0;

                try
                {
                    object value = Microsoft.Win32.Registry.GetValue(
                        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                        "AppsUseLightTheme",
                        1);

                    if (value is int && (int)value == 0)
                    {
                        _detectedTheme = 1;
                    }
                }
                catch
                {
                    // Registry unavailable -> assume light.
                }
            }

            return _detectedTheme == 1
                ? HMessageThemeMode.Dark
                : HMessageThemeMode.Light;
        }

        internal static void PlaySoundFor(HMessageType type)
        {
            try
            {
                switch (type)
                {
                    case HMessageType.Information:
                    case HMessageType.Success:
                        SystemSounds.Asterisk.Play();
                        break;


                    case HMessageType.Error:
                    case HMessageType.Delete:
                        SystemSounds.Hand.Play();
                        break;

                    case HMessageType.Warning:
                    case HMessageType.Exit:
                        SystemSounds.Exclamation.Play();
                        break;

                    case HMessageType.Question:
                    case HMessageType.Hide:
                        SystemSounds.Question.Play();
                        break;
                }
            }
            catch
            {
                // Sound must never break the dialog.
            }
        }

        #endregion

        #region General Show

        /// <summary>
        /// Shows a fully customized dialog. This is the primary entry point.
        /// </summary>
        public static DialogResult Show(
            IWin32Window owner,
            HMessageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            using (HMessageForm dialog = new HMessageForm())
            {
                dialog.Configure(options);

                return owner == null
                    ? dialog.ShowDialog()
                    : dialog.ShowDialog(owner);
            }
        }

        public static DialogResult Show(HMessageOptions options)
        {
            return Show(null, options);
        }

        public static DialogResult Show(
            IWin32Window owner,
            string message,
            string caption,
            HMessageType type,
            HMessageButtons buttons)
        {
            return Show(
                owner,
                new HMessageOptions
                {
                    Message = message,
                    Caption = caption,
                    Type = type,
                    Buttons = buttons
                });
        }

        public static DialogResult Show(
            string message,
            string caption,
            HMessageType type,
            HMessageButtons buttons)
        {
            return Show(null, message, caption, type, buttons);
        }

        #endregion

        #region Information

        public static DialogResult ShowInformation(
            IWin32Window owner,
            string message,
            string caption)
        {
            return Show(
                owner,
                message,
                caption,
                HMessageType.Information,
                HMessageButtons.Ok);
        }

        public static DialogResult ShowInformation(
            string message,
            string caption)
        {
            return ShowInformation(null, message, caption);
        }

        #endregion

        #region Success

        public static DialogResult ShowSuccess(
            IWin32Window owner,
            string message,
            string caption)
        {
            return Show(
                owner,
                message,
                caption,
                HMessageType.Success,
                HMessageButtons.Ok);
        }

        public static DialogResult ShowSuccess(
            string message,
            string caption)
        {
            return ShowSuccess(null, message, caption);
        }

        #endregion

        #region Warning

        public static DialogResult ShowWarning(
            IWin32Window owner,
            string message,
            string caption)
        {
            return Show(
                owner,
                message,
                caption,
                HMessageType.Warning,
                HMessageButtons.Ok);
        }

        public static DialogResult ShowWarning(
            string message,
            string caption)
        {
            return ShowWarning(null, message, caption);
        }

        #endregion

        #region Error

        public static DialogResult ShowError(
            IWin32Window owner,
            string message,
            string caption)
        {
            // NOTE: previously this called ShowError("", message, caption),
            // which silently bound to the (string, string, string) overload
            // and displayed an empty message with the caption as details.
            return Show(
                owner,
                message,
                caption,
                HMessageType.Error,
                HMessageButtons.Ok);
        }

        public static DialogResult ShowError(
            string message,
            string caption)
        {
            return ShowError((IWin32Window)null, message, caption);
        }

        public static DialogResult ShowError(
            IWin32Window owner,
            string message,
            string caption,
            string details)
        {
            return Show(
                owner,
                new HMessageOptions
                {
                    Message = message,
                    Caption = caption,
                    Type = HMessageType.Error,
                    Buttons = HMessageButtons.Ok,
                    Details = details
                });
        }

        public static DialogResult ShowError(
            string message,
            string caption,
            string details)
        {
            return ShowError((IWin32Window)null, message, caption, details);
        }

        public static DialogResult ShowException(
            IWin32Window owner,
            string message,
            string caption,
            Exception exception)
        {
            return ShowError(
                owner,
                message,
                caption,
                BuildExceptionDetails(exception));
        }

        public static DialogResult ShowException(
            string message,
            string caption,
            Exception exception)
        {
            return ShowException(null, message, caption, exception);
        }

        #endregion

        #region Question

        public static DialogResult ShowQuestion(
            IWin32Window owner,
            string message,
            string caption)
        {
            return Show(
                owner,
                message,
                caption,
                HMessageType.Question,
                HMessageButtons.YesNo);
        }

        public static DialogResult ShowQuestion(
            string message,
            string caption)
        {
            return ShowQuestion(null, message, caption);
        }

        public static DialogResult ShowQuestion(
            IWin32Window owner,
            string message,
            string caption,
            HMessageButtons buttons)
        {
            return Show(
                owner,
                new HMessageOptions
                {
                    Message = message,
                    Caption = caption,
                    Type = HMessageType.Question,
                    Buttons = buttons
                });
        }

        #endregion

        #region Delete

        public static DialogResult ShowDelete(IWin32Window owner, string message, string caption)
        {
            return Show(owner, message, caption, HMessageType.Delete, HMessageButtons.YesNo);
        }

        public static DialogResult ShowDelete(string message, string caption)
        {
            return ShowDelete(null, message, caption);
        }

        #endregion

        #region Exit

        public static DialogResult ShowExit(IWin32Window owner, string message, string caption)
        {
            return Show(owner, message, caption, HMessageType.Exit, HMessageButtons.YesNo);
        }

        public static DialogResult ShowExit(string message, string caption)
        {
            return ShowExit(null, message, caption);
        }

        #endregion

        #region Hide

        public static DialogResult ShowHide(IWin32Window owner, string message, string caption)
        {
            return Show(owner, message, caption, HMessageType.Hide, HMessageButtons.YesNo);
        }

        public static DialogResult ShowHide(string message, string caption)
        {
            return ShowHide(null, message, caption);
        }

        #endregion

        #region Internal Helpers

        private static string BuildExceptionDetails(Exception exception)
        {
            return exception == null ? null : exception.ToString();
        }

        /// <summary>
        /// Writes to the clipboard with a small retry loop: the clipboard is a
        /// shared resource and may be owned by another process for a moment.
        /// </summary>
        internal static bool TrySetClipboardText(string text)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return true;
                }
                catch (ExternalException)
                {
                    System.Threading.Thread.Sleep(70);
                }
            }

            return false;
        }

        #endregion
    }

    #endregion

    #region Dialog Form

    internal sealed class HMessageForm : Form
    {
        #region Native Constants

        private const int CS_DROPSHADOW = 0x00020000;

        #endregion

        #region Layout Constants (design px @ 96 DPI)

        private const int DesignWidth = 560;
        private const int MinWidth = 400;
        private const int MaxWidth = 760;

        private const int MinHeight = 200;
        private const int MaxHeight = 720;

        private const int CornerRadius = 12;
        private const int AccentBarHeight = 5;
        private const int OuterPadding = 24;
        private const int ContentGap = 14;

        private const int IconColumnWidth = 74;
        private const int IconSize = 52;

        private const int ButtonHeight = 40;
        private const int ButtonMinWidth = 108;
        private const int ButtonMaxWidth = 260;
        private const int ButtonBarHeight = 56;

        private const int DetailsHeight = 190;
        private const int MessageMaxHeight = 300;

        private const int CaptionFontPx = 20;
        private const int MessageFontPx = 15;
        private const int DetailsFontPx = 13;
        private const int ButtonFontPx = 15;

        #endregion

        #region Fields

        private readonly BufferedTableLayoutPanel _rootLayout;
        private readonly Panel _accentBar;

        private readonly BufferedPanel _messagePanel;
        private readonly BufferedTableLayoutPanel _messageLayout;
        private readonly HMessageIconControl _iconControl;

        private readonly BufferedTableLayoutPanel _textLayout;
        private readonly Label _lblCaption;
        private readonly Label _lblMessage;

        private readonly CheckBox _chkVerification;

        private readonly BufferedPanel _detailsContainer;
        private readonly BufferedTableLayoutPanel _detailsLayout;
        private readonly HMessageButton _btnDetails;
        private readonly TextBox _txtDetails;
        private readonly HMessageButton _btnCopyDetails;

        private readonly BufferedPanel _buttonContainer;
        private readonly BufferedFlowLayoutPanel _buttonLayout;

        private readonly HMessageButton _btnPrimary;
        private readonly HMessageButton _btnSecondary;
        private readonly HMessageButton _btnTertiary;

        private readonly Timer _animationTimer;
        private readonly Timer _countdownTimer;
        private readonly Timer _copyFeedbackTimer;

        private GraphicsPath _windowPath;
        private Pen _borderPen;

        private HMessageOptions _options;
        private HMessagePalette _palette;
        private Color _accent;

        private Font _captionFont;
        private Font _messageFont;
        private Font _detailsFont;
        private Font _buttonFont;

        private int _dpi = 96;
        private bool _nativeRounding;

        private bool _detailsVisible;
        private bool _closing;
        private bool _allowClose;

        private double _targetOpacity;
        private double _currentOpacity;

        private DialogResult _pendingResult;
        private DialogResult _defaultResult;
        private DialogResult _escapeResult;

        private HMessageButton _focusButton;
        private HMessageButton _countdownButton;
        private string _countdownBaseText;
        private int _countdownRemaining;

        #endregion

        #region Constructor

        public HMessageForm()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            UpdateStyles();

            // Everything in this dialog is scaled manually against _dpi.
            // AutoScaleMode.None keeps that deterministic (no double scaling).
            AutoScaleMode = AutoScaleMode.None;

            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            ShowInTaskbar = false;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;

            StartPosition = FormStartPosition.Manual;

            KeyPreview = true;

            _palette = HMessage.ResolvePalette(HMessageThemeMode.System);
            _accent = _palette.Information;

            _dpi = DeviceDpi;

            CreateFonts();

            BackColor = _palette.Surface;
            ForeColor = _palette.TextPrimary;
            Font = _messageFont;

            _borderPen = new Pen(_palette.Border, Scale(1));

            // ---------------------------------------------------------
            // Root layout
            // ---------------------------------------------------------

            _rootLayout = new BufferedTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _palette.Surface,
                // 1px ring so the painted window border stays visible.
                Margin = new Padding(Scale(1)),
                ColumnCount = 1,
                RowCount = 5
            };

            _rootLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            for (int i = 0; i < 5; i++)
            {
                _rootLayout.RowStyles.Add(
                    new RowStyle(SizeType.AutoSize));
            }

            _accentBar = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = _accent,
                Margin = Padding.Empty
            };

            // ---------------------------------------------------------
            // Message area (scrollable when the text is very long)
            // ---------------------------------------------------------

            _messagePanel = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoScroll = true,
                BackColor = _palette.Surface,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                TabStop = false
            };

            _messageLayout = new BufferedTableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                BackColor = _palette.Surface,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 2,
                RowCount = 1
            };

            _messageLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    Scale(IconColumnWidth)));

            _messageLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            _iconControl = new HMessageIconControl
            {
                Dock = DockStyle.Top,
                TabStop = false,
                AccessibleRole = AccessibleRole.Graphic
            };

            _textLayout = new BufferedTableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 1,
                RowCount = 2
            };

            _textLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            _textLayout.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _textLayout.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _lblCaption = new Label
            {
                AutoSize = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Font = _captionFont,
                ForeColor = _palette.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            };

            _lblMessage = new Label
            {
                AutoSize = true,
                Margin = new Padding(0, Scale(8), 0, 0),
                Padding = Padding.Empty,
                Font = _messageFont,
                ForeColor = _palette.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false
            };

            _textLayout.Controls.Add(_lblCaption, 0, 0);
            _textLayout.Controls.Add(_lblMessage, 0, 1);

            _messageLayout.Controls.Add(_iconControl, 0, 0);
            _messageLayout.Controls.Add(_textLayout, 1, 0);

            _messagePanel.Controls.Add(_messageLayout);

            // ---------------------------------------------------------
            // Verification check-box ("Don't show again", ...)
            // ---------------------------------------------------------

            _chkVerification = new CheckBox
            {
                AutoSize = true,
                Visible = false,
                FlatStyle = FlatStyle.Standard,
                ForeColor = _palette.TextSecondary,
                BackColor = Color.Transparent,
                Font = _messageFont,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, Scale(ContentGap)),
                TabIndex = 3
            };

            // ---------------------------------------------------------
            // Details area
            // ---------------------------------------------------------

            _detailsContainer = new BufferedPanel
            {
                Visible = false,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Margin = new Padding(0, 0, 0, Scale(ContentGap)),
                Padding = Padding.Empty,
                BackColor = _palette.SurfaceSecondary
            };

            _detailsContainer.Paint += detailsContainer_Paint;

            _detailsLayout = new BufferedTableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(Scale(12)),
                ColumnCount = 2,
                RowCount = 1,
                // Never mirrored: the details content is technical text
                // (stack traces, logs) which stays LTR. Mirroring used to
                // flip the copy-button margin to the wrong side so the
                // text overlapped the button.
                RightToLeft = RightToLeft.No
            };

            _detailsLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            _detailsLayout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    Scale(112)));

            _txtDetails = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = _palette.SurfaceSecondary,
                ForeColor = _palette.DetailsText,
                Font = _detailsFont,
                Margin = Padding.Empty,
                AccessibleName = "Details",
                // Stack traces / logs are technical LTR content.
                RightToLeft = RightToLeft.No
            };

            _btnCopyDetails = new HMessageButton
            {
                ButtonKind = HMessageButtonKind.Secondary,
                Dock = DockStyle.Top,
                Margin = new Padding(Scale(12), 0, 0, 0),
                TabIndex = 4
            };

            _btnCopyDetails.Click += btnCopyDetails_Click;

            _detailsLayout.Controls.Add(_txtDetails, 0, 0);
            _detailsLayout.Controls.Add(_btnCopyDetails, 1, 0);

            _detailsContainer.Controls.Add(_detailsLayout);

            // ---------------------------------------------------------
            // Details toggle link
            // ---------------------------------------------------------

            _btnDetails = new HMessageButton
            {
                Visible = false,
                AutoSize = true,
                ButtonKind = HMessageButtonKind.Link,
                Margin = new Padding(0, 0, 0, Scale(ContentGap)),
                Padding = Padding.Empty,
                TabIndex = 2
            };

            _btnDetails.Click += btnDetails_Click;

            // ---------------------------------------------------------
            // Action buttons
            // ---------------------------------------------------------

            _buttonContainer = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _palette.Surface,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _buttonLayout = new BufferedFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                // Deterministic order: we control direction explicitly and
                // never rely on form-level RTL mirroring for the button row.
                RightToLeft = RightToLeft.No,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                BackColor = Color.Transparent
            };

            _btnPrimary = CreateActionButton(0);
            _btnSecondary = CreateActionButton(1);
            _btnTertiary = CreateActionButton(2);

            _btnPrimary.Click += btnAction_Click;
            _btnSecondary.Click += btnAction_Click;
            _btnTertiary.Click += btnAction_Click;

            _buttonContainer.Controls.Add(_buttonLayout);

            // ---------------------------------------------------------
            // Assemble
            // ---------------------------------------------------------

            _rootLayout.Controls.Add(_messagePanel, 0, 0);
            _rootLayout.Controls.Add(_chkVerification, 0, 1);
            _rootLayout.Controls.Add(_btnDetails, 0, 2);
            _rootLayout.Controls.Add(_detailsContainer, 0, 3);
            _rootLayout.Controls.Add(_buttonContainer, 0, 4);

            Controls.Add(_rootLayout);
            Controls.Add(_accentBar); // docks Top before the fill layout

            // ---------------------------------------------------------
            // Timers
            // ---------------------------------------------------------

            _animationTimer = new Timer { Interval = 15 };
            _animationTimer.Tick += animationTimer_Tick;

            _countdownTimer = new Timer { Interval = 1000 };
            _countdownTimer.Tick += countdownTimer_Tick;

            _copyFeedbackTimer = new Timer { Interval = 1400 };
            _copyFeedbackTimer.Tick += copyFeedbackTimer_Tick;

            Shown += HMessageDialogForm_Shown;
            FormClosed += HMessageDialogForm_FormClosed;
            KeyDown += HMessageDialogForm_KeyDown;
            DpiChanged += HMessageDialogForm_DpiChanged;

            ApplyDpiMetrics();
        }

        #endregion

        #region DPI

        private int Scale(int value)
        {
            return Math.Max(
                1,
                (int)Math.Round(value * (_dpi / 96f)));
        }

        /// <summary>
        /// (Re)applies every metric that depends on the DPI. Centralizing this
        /// keeps per-monitor scaling correct when DpiChanged fires.
        /// </summary>
        private void ApplyDpiMetrics()
        {
            _borderPen.Width = Scale(1);

            MinimumSize = new Size(Scale(MinWidth), Scale(MinHeight));

            _rootLayout.Padding = new Padding(Scale(OuterPadding));
            _rootLayout.Margin = new Padding(Scale(1));

            _accentBar.Height = Scale(AccentBarHeight);

            _messagePanel.Margin = new Padding(
                0, Scale(10), 0, Scale(ContentGap));

            _messageLayout.ColumnStyles[0].Width =
                Scale(IconColumnWidth);

            _iconControl.Size = new Size(Scale(IconSize), Scale(IconSize));

            ApplyIconMargin();

            _lblMessage.Margin = new Padding(0, Scale(8), 0, 0);

            _lblCaption.Font = _captionFont;
            _lblMessage.Font = _messageFont;
            _chkVerification.Font = _messageFont;
            _txtDetails.Font = _detailsFont;

            _chkVerification.Margin =
                new Padding(0, 0, 0, Scale(ContentGap));

            _detailsContainer.Height = Scale(DetailsHeight);
            _detailsContainer.Margin =
                new Padding(0, 0, 0, Scale(ContentGap));

            _detailsLayout.Padding = new Padding(Scale(12));
            _detailsLayout.ColumnStyles[1].Width = Scale(112);

            _btnCopyDetails.Margin =
                new Padding(Scale(12), 0, 0, 0);

            _btnDetails.Margin =
                new Padding(0, 0, 0, Scale(ContentGap));

            _btnDetails.MinimumSize = new Size(0, Scale(28));

            _buttonContainer.Height = Scale(ButtonBarHeight);

            ApplyButtonFont();
        }

        private void ApplyIconMargin()
        {
            int gap = Scale(IconColumnWidth - IconSize - 6);

            // The gap belongs between the icon and the text: right of the
            // icon in LTR, left of the icon in RTL (margins are physical
            // and are not mirrored automatically).
            _iconControl.Margin = HMessage.IsArabic
                ? new Padding(gap, Scale(2), 0, 0)
                : new Padding(0, Scale(2), gap, 0);
        }

        private void ApplyButtonFont()
        {
            int vertical = Math.Max(
                0,
                (Scale(ButtonBarHeight) - Scale(ButtonHeight)) / 2);

            HMessageButton[] buttons =
            {
                _btnPrimary,
                _btnSecondary,
                _btnTertiary,
                _btnCopyDetails,
                _btnDetails
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Font = _buttonFont;

                if (buttons[i] != _btnDetails)
                {
                    buttons[i].Margin = new Padding(
                        Scale(4), vertical, Scale(4), 0);
                }
            }

            _btnCopyDetails.Size = new Size(Scale(100), Scale(34));
            _btnCopyDetails.Margin =
                new Padding(Scale(12), 0, 0, 0);
        }

        private void HMessageDialogForm_DpiChanged(
            object sender,
            DpiChangedEventArgs e)
        {
            if (_dpi == DeviceDpi)
            {
                return;
            }

            _dpi = DeviceDpi;

            RecreateFonts();
            ApplyDpiMetrics();

            if (_options != null)
            {
                ApplyButtons();
            }

            UpdateDialogSize();
            UpdateWindowRegion();
        }

        #endregion

        #region Fonts

        private void CreateFonts()
        {
            _captionFont = CreateFontSafe(
                "Segoe UI", Scale(CaptionFontPx), FontStyle.Bold, null);

            _messageFont = CreateFontSafe(
                "Segoe UI", Scale(MessageFontPx), FontStyle.Regular, null);

            _detailsFont = CreateFontSafe(
                "Consolas", Scale(DetailsFontPx), FontStyle.Regular,
                "Courier New");

            _buttonFont = CreateFontSafe(
                "Segoe UI Semibold", Scale(ButtonFontPx),
                FontStyle.Regular, "Segoe UI");
        }

        private void RecreateFonts()
        {
            Font oldCaption = _captionFont;
            Font oldMessage = _messageFont;
            Font oldDetails = _detailsFont;
            Font oldButton = _buttonFont;

            CreateFonts();

            _lblCaption.Font = _captionFont;
            _lblMessage.Font = _messageFont;
            _chkVerification.Font = _messageFont;
            _txtDetails.Font = _detailsFont;
            Font = _messageFont;

            oldCaption.Dispose();
            oldMessage.Dispose();
            oldDetails.Dispose();
            oldButton.Dispose();
        }

        private static Font CreateFontSafe(
            string family,
            float pixelSize,
            FontStyle style,
            string fallback)
        {
            string[] chain = fallback == null
                ? new[] { family, "Segoe UI", "Tahoma" }
                : new[] { family, fallback, "Segoe UI", "Tahoma" };

            for (int i = 0; i < chain.Length; i++)
            {
                try
                {
                    return new Font(
                        chain[i],
                        pixelSize,
                        style,
                        GraphicsUnit.Pixel);
                }
                catch (ArgumentException)
                {
                    // Family not installed -> try the next one.
                }
            }

            return new Font(
                FontFamily.GenericSansSerif,
                pixelSize,
                style,
                GraphicsUnit.Pixel);
        }

        #endregion

        #region Configuration

        public void Configure(HMessageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _options = options;
            _palette = HMessage.ResolvePalette(options.Theme);

            SuspendLayout();

            try
            {
                ApplyTheme();
                ApplyLanguage();
                ApplyMessage();
                ApplyMessageType();
                ApplyVerification();
                ApplyDetails();
                ApplyButtons();
                ApplyCountdown();
            }
            finally
            {
                ResumeLayout(true);
            }

            UpdateDialogSize();
            UpdateWindowRegion();
        }

        private void ApplyTheme()
        {
            BackColor = _palette.Surface;
            ForeColor = _palette.TextPrimary;

            _rootLayout.BackColor = _palette.Surface;
            _messagePanel.BackColor = _palette.Surface;
            _messageLayout.BackColor = _palette.Surface;
            _buttonContainer.BackColor = _palette.Surface;

            _lblCaption.ForeColor = _palette.TextPrimary;
            _lblMessage.ForeColor = _palette.TextSecondary;

            _chkVerification.ForeColor = _palette.TextSecondary;

            _detailsContainer.BackColor = _palette.SurfaceSecondary;
            _txtDetails.BackColor = _palette.SurfaceSecondary;
            _txtDetails.ForeColor = _palette.DetailsText;

            _borderPen.Color = _palette.Border;

            _iconControl.Palette = _palette;

            _btnPrimary.Palette = _palette;
            _btnSecondary.Palette = _palette;
            _btnTertiary.Palette = _palette;
            _btnCopyDetails.Palette = _palette;
            _btnDetails.Palette = _palette;
        }

        private void ApplyLanguage()
        {
            bool arabic = HMessage.IsArabic;

            RightToLeft = arabic ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = arabic;

            // LTR: text hugs the left side.  RTL: text hugs the right side.
            // AutoSize labels must also be *anchored* to that side —
            // TextAlign alone only affects lines inside the label bounds.
            ContentAlignment textAlign = arabic
                ? ContentAlignment.TopRight
                : ContentAlignment.TopLeft;

            AnchorStyles anchor = arabic
                ? AnchorStyles.Top | AnchorStyles.Left
                : AnchorStyles.Top | AnchorStyles.Left;

            _lblCaption.TextAlign = textAlign;
            _lblMessage.TextAlign = textAlign;


            _lblCaption.Anchor = anchor;
            _lblMessage.Anchor = anchor;
            _btnDetails.Anchor = anchor;
            _chkVerification.Anchor = anchor;

            ApplyIconMargin();

            // Button row direction is owned by ApplyButtons()
            // (Windows convention: LTR right-aligned, RTL left-aligned).

            _btnCopyDetails.Text = HMessage.GetText(
                "Copy details", "نسخ التفاصيل");

            UpdateDetailsLinkText();
        }

        private void ApplyMessage()
        {
            string caption = _options.Caption ?? string.Empty;
            string message = _options.Message ?? string.Empty;

            _lblCaption.Text = caption;
            _lblMessage.Text = message;

            Text = string.IsNullOrEmpty(caption) ? message : caption;
            AccessibleName = Text;
            AccessibleDescription = message;
        }

        private void ApplyMessageType()
        {
            switch (_options.Type)
            {
                case HMessageType.Information:
                    _accent = _palette.Information;
                    break;

                case HMessageType.Success:
                    _accent = _palette.Success;
                    break;

                case HMessageType.Warning:
                    _accent = _palette.Warning;
                    break;

                case HMessageType.Error:
                    _accent = _palette.Error;
                    break;

                case HMessageType.Question:
                    _accent = _palette.Question;
                    break;

                case HMessageType.Delete:
                    _accent = _palette.Delete;
                    break;

                case HMessageType.Exit:
                    _accent = _palette.Exit;
                    break;

                case HMessageType.Hide:
                    _accent = _palette.Hide;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        "Type", _options.Type, null);
            }

            _accentBar.BackColor = _accent;

            _iconControl.MessageType = _options.Type;
            _iconControl.AccentColor = _accent;
            _iconControl.AccessibleName = _options.Type.ToString();

            _btnPrimary.AccentColor = _accent;
            _btnSecondary.AccentColor = _accent;
            _btnTertiary.AccentColor = _accent;
        }

        private void ApplyVerification()
        {
            bool visible =
                !string.IsNullOrEmpty(_options.VerificationText);

            _chkVerification.Visible = visible;

            if (visible)
            {
                _chkVerification.Text = _options.VerificationText;
                _chkVerification.Checked = _options.VerificationChecked;
            }
        }

        private void ApplyDetails()
        {
            bool hasDetails =
                !string.IsNullOrWhiteSpace(_options.Details);

            _btnDetails.Visible = hasDetails;

            if (!hasDetails)
            {
                _detailsVisible = false;
                _detailsContainer.Visible = false;
                _txtDetails.Clear();
                return;
            }

            _txtDetails.Text = _options.Details;
            _txtDetails.Select(0, 0);

            _detailsVisible = _options.DetailsExpanded;
            _detailsContainer.Visible = _detailsVisible;

            UpdateDetailsLinkText();
        }

        private void UpdateDetailsLinkText()
        {
            _btnDetails.Text = HMessage.GetText(
                _detailsVisible ? "Hide details  ▲" : "Show details  ▼",
                _detailsVisible ? "إخفاء التفاصيل  ▲" : "عرض التفاصيل  ▼");
        }

        #endregion

        #region Buttons

        private HMessageButton CreateActionButton(int tabIndex)
        {
            return new HMessageButton
            {
                Size = new Size(Scale(ButtonMinWidth), Scale(ButtonHeight)),
                TabIndex = tabIndex
            };
        }

        private void ApplyButtons()
        {
            _buttonLayout.SuspendLayout();
            _buttonLayout.Controls.Clear();

            bool arabic = HMessage.IsArabic;

            // Windows convention:
            //   LTR -> buttons right-aligned, primary in the bottom-right corner.
            //   RTL -> buttons left-aligned,  primary in the bottom-left corner.
            // FlowLayoutPanel starts stacking from the edge named by
            // FlowDirection, so adding primary first always puts it in
            // the corner. RightToLeftLayout mirroring is disabled on this
            // panel (RightToLeft.No), so this mapping is deterministic.
            _buttonLayout.FlowDirection = arabic
                ? FlowDirection.LeftToRight
                : FlowDirection.RightToLeft;

            _btnPrimary.Visible = false;
            _btnSecondary.Visible = false;
            _btnTertiary.Visible = false;

            _btnPrimary.ButtonKind = HMessageButtonKind.Primary;
            _btnSecondary.ButtonKind = HMessageButtonKind.Secondary;
            _btnTertiary.ButtonKind = HMessageButtonKind.Secondary;

            string ok = _options.PrimaryButtonText ??
                HMessage.GetText("OK", "موافق");

            string yes = _options.PrimaryButtonText ??
                HMessage.GetText("Yes", "نعم");

            string no = _options.SecondaryButtonText ??
                HMessage.GetText("No", "لا");

            string cancel = (_options.Buttons == HMessageButtons.YesNoCancel
                    ? _options.TertiaryButtonText
                    : _options.SecondaryButtonText) ??
                HMessage.GetText("Cancel", "إلغاء");

            switch (_options.Buttons)
            {
                case HMessageButtons.Ok:
                    PrepareButton(_btnPrimary, ok, DialogResult.OK);
                    break;

                case HMessageButtons.OkCancel:
                    PrepareButton(_btnPrimary, ok, DialogResult.OK);
                    PrepareButton(
                        _btnSecondary, cancel, DialogResult.Cancel);
                    break;

                case HMessageButtons.YesNo:
                    PrepareButton(_btnPrimary, yes, DialogResult.Yes);
                    PrepareButton(_btnSecondary, no, DialogResult.No);
                    break;

                case HMessageButtons.YesNoCancel:
                    PrepareButton(_btnPrimary, yes, DialogResult.Yes);
                    PrepareButton(_btnSecondary, no, DialogResult.No);
                    PrepareButton(
                        _btnTertiary, cancel, DialogResult.Cancel);
                    break;

                default:
                    _buttonLayout.ResumeLayout(true);
                    throw new ArgumentOutOfRangeException(
                        "Buttons", _options.Buttons, null);
            }

            // Primary is added first so it lands in the corner
            // (bottom-right in LTR, bottom-left in RTL).
            AddVisible(_btnPrimary);
            AddVisible(_btnSecondary);
            AddVisible(_btnTertiary);

            _defaultResult = ResolveDefaultResult();
            _escapeResult = ResolveEscapeResult();

            _focusButton = FindButton(_defaultResult) ?? _btnPrimary;

            HMessageButton accept = FindButton(_defaultResult);
            HMessageButton cancelButton = FindButton(_escapeResult);

            AcceptButton = accept;
            CancelButton = cancelButton;

            _buttonLayout.ResumeLayout(true);
        }

        private void AddVisible(HMessageButton button)
        {
            if (button != null && button.Visible)
            {
                _buttonLayout.Controls.Add(button);
            }
        }

        private HMessageButton FindButton(DialogResult result)
        {
            HMessageButton[] buttons =
                { _btnPrimary, _btnSecondary, _btnTertiary };

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].Visible &&
                    buttons[i].Tag is DialogResult &&
                    (DialogResult)buttons[i].Tag == result)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private DialogResult ResolveDefaultResult()
        {
            if (_options.DefaultResult != DialogResult.None &&
                FindButton(_options.DefaultResult) != null)
            {
                return _options.DefaultResult;
            }

            switch (_options.Buttons)
            {
                case HMessageButtons.YesNo:
                case HMessageButtons.YesNoCancel:
                    return DialogResult.Yes;

                default:
                    return DialogResult.OK;
            }
        }

        private DialogResult ResolveEscapeResult()
        {
            switch (_options.Buttons)
            {
                case HMessageButtons.Ok:
                    return DialogResult.OK;

                case HMessageButtons.OkCancel:
                case HMessageButtons.YesNoCancel:
                    return DialogResult.Cancel;

                case HMessageButtons.YesNo:
                    return DialogResult.No;

                default:
                    return DialogResult.Cancel;
            }
        }

        private void PrepareButton(
            HMessageButton button,
            string text,
            DialogResult result)
        {
            button.Text = text;
            button.Tag = result;
            button.Visible = true;
            button.Size = new Size(
                MeasureButtonWidth(text),
                Scale(ButtonHeight));
        }

        private int MeasureButtonWidth(string text)
        {
            Size measured = TextRenderer.MeasureText(
                text ?? string.Empty,
                _buttonFont,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding);

            int width = measured.Width + Scale(36);

            return Math.Max(
                Scale(ButtonMinWidth),
                Math.Min(Scale(ButtonMaxWidth), width));
        }

        #endregion

        #region Button Handling

        private void btnAction_Click(object sender, EventArgs e)
        {
            HMessageButton button = sender as HMessageButton;

            DialogResult result =
                button != null && button.Tag is DialogResult
                    ? (DialogResult)button.Tag
                    : DialogResult.None;

            CompleteWithResult(result);
        }

        private void CompleteWithResult(DialogResult result)
        {
            if (_closing)
            {
                return;
            }

            if (result == DialogResult.None)
            {
                result = _defaultResult;
            }

            _pendingResult = result;

            BeginFadeOut();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_options.Details))
            {
                return;
            }

            _detailsVisible = !_detailsVisible;
            _detailsContainer.Visible = _detailsVisible;

            UpdateDetailsLinkText();
            UpdateDialogSize();
            ClampToWorkingArea();
        }

        private void btnCopyDetails_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_txtDetails.Text))
            {
                return;
            }

            if (HMessage.TrySetClipboardText(_txtDetails.Text))
            {
                _copyFeedbackTimer.Stop();

                _btnCopyDetails.Text = HMessage.GetText(
                    "Copied!", "تم النسخ!");

                _copyFeedbackTimer.Start();
            }
        }

        private void copyFeedbackTimer_Tick(object sender, EventArgs e)
        {
            _copyFeedbackTimer.Stop();

            _btnCopyDetails.Text = HMessage.GetText(
                "Copy details", "نسخ التفاصيل");
        }

        #endregion

        #region Keyboard

        private void HMessageDialogForm_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (_closing || _options == null)
            {
                return;
            }

            // Ctrl+C copies a formatted report (MessageBox parity).
            // When the details box has focus, let it copy its selection.
            if (e.Control &&
                e.KeyCode == Keys.C &&
                _options.AllowCopyShortcut &&
                !ReferenceEquals(ActiveControl, _txtDetails))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CopyReportToClipboard();
            }
        }

        private void CopyReportToClipboard()
        {
            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(_lblCaption.Text))
            {
                builder.AppendLine(_lblCaption.Text);
            }

            if (!string.IsNullOrEmpty(_lblMessage.Text))
            {
                builder.AppendLine(_lblMessage.Text);
            }

            if (_detailsVisible &&
                !string.IsNullOrEmpty(_txtDetails.Text))
            {
                builder.AppendLine();
                builder.AppendLine(_txtDetails.Text);
            }

            if (builder.Length > 0)
            {
                HMessage.TrySetClipboardText(builder.ToString());
            }
        }

        #endregion

        #region Auto-close Countdown

        private void ApplyCountdown()
        {
            _countdownTimer.Stop();
            _countdownButton = null;

            if (_options.TimeoutSeconds <= 0)
            {
                return;
            }

            _countdownButton = FindButton(_defaultResult);

            if (_countdownButton == null)
            {
                return;
            }

            _countdownRemaining = _options.TimeoutSeconds;
            _countdownBaseText = _countdownButton.Text;

            UpdateCountdownText();
            _countdownTimer.Start();
        }

        private void UpdateCountdownText()
        {
            if (_countdownButton == null)
            {
                return;
            }

            _countdownButton.Text = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0} ({1})",
                _countdownBaseText,
                _countdownRemaining);

            int width = MeasureButtonWidth(_countdownButton.Text);

            if (width > _countdownButton.Width)
            {
                _countdownButton.Width = width;
            }
        }

        private void countdownTimer_Tick(object sender, EventArgs e)
        {
            if (_closing)
            {
                _countdownTimer.Stop();
                return;
            }

            _countdownRemaining--;

            if (_countdownRemaining <= 0)
            {
                _countdownTimer.Stop();
                CompleteWithResult(_defaultResult);
                return;
            }

            UpdateCountdownText();
        }

        #endregion

        #region Animation

        private void HMessageDialogForm_Shown(object sender, EventArgs e)
        {
            CenterOnOwner();

            if (_focusButton != null)
            {
                // Focus ring only appears once the user presses a key
                // (ShowFocusCues), so mouse users see a clean dialog.
                ActiveControl = _focusButton;
                _focusButton.Select();
            }

            if (_options != null && _options.PlaySound)
            {
                HMessage.PlaySoundFor(_options.Type);
            }

            if (!HMessage.IsAnimationEnabled)
            {
                SafeSetOpacity(1.0);
                return;
            }

            _currentOpacity = 0.0;
            _targetOpacity = 1.0;

            SafeSetOpacity(_currentOpacity);

            _animationTimer.Start();
        }

        private void BeginFadeOut()
        {
            _countdownTimer.Stop();

            if (!HMessage.IsAnimationEnabled)
            {
                CloseDialogNow();
                return;
            }

            _closing = true;

            _currentOpacity = Opacity;
            _targetOpacity = 0.0;

            _animationTimer.Start();
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            double difference = _targetOpacity - _currentOpacity;

            // Exponential ease -> smooth, frame-rate independent fade.
            if (Math.Abs(difference) < 0.02)
            {
                _currentOpacity = _targetOpacity;

                SafeSetOpacity(_currentOpacity);

                _animationTimer.Stop();

                if (_closing)
                {
                    CloseDialogNow();
                }

                return;
            }

            _currentOpacity += difference * 0.35;

            SafeSetOpacity(_currentOpacity);
        }

        private void SafeSetOpacity(double value)
        {
            try
            {
                Opacity = value;
            }
            catch
            {
                // Some platforms/remote sessions cannot change opacity.
            }
        }

        private void CloseDialogNow()
        {
            _animationTimer.Stop();
            _countdownTimer.Stop();

            // Hand the verification state back to the caller.
            if (_options != null &&
                !string.IsNullOrEmpty(_options.VerificationText))
            {
                _options.VerificationChecked = _chkVerification.Checked;
            }

            _allowClose = true;

            DialogResult result = _pendingResult;

            if (result == DialogResult.None)
            {
                result = _escapeResult;
            }

            DialogResult = result;

            Close();
        }

        #endregion

        #region Form Lifecycle

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Windows 11+: let the DWM round the corners natively
            // (crisper edges + real shadow). Falls back to Region below.
            _nativeRounding = NativeMethods.TryEnableRoundedCorners(Handle);

            if (_nativeRounding)
            {
                Region old = Region;
                Region = null;

                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UpdateDialogSize();
            UpdateWindowRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateWindowRegion();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                if (_closing)
                {
                    e.Cancel = true;
                    return;
                }

                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // Alt+F4 / system menu -> behave like Escape and close
                    // with the matching result instead of DialogResult.None.
                    e.Cancel = true;
                    CompleteWithResult(_escapeResult);
                    return;
                }

                // Application shutdown / programmatic Close().
                _animationTimer.Stop();
                _countdownTimer.Stop();
            }

            base.OnFormClosing(e);
        }

        private void HMessageDialogForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            _animationTimer.Stop();
            _countdownTimer.Stop();
            _copyFeedbackTimer.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_animationTimer != null)
                {
                    _animationTimer.Stop();
                    _animationTimer.Dispose();
                }

                if (_countdownTimer != null)
                {
                    _countdownTimer.Stop();
                    _countdownTimer.Dispose();
                }

                if (_copyFeedbackTimer != null)
                {
                    _copyFeedbackTimer.Stop();
                    _copyFeedbackTimer.Dispose();
                }

                if (_borderPen != null)
                {
                    _borderPen.Dispose();
                    _borderPen = null;
                }

                if (_windowPath != null)
                {
                    _windowPath.Dispose();
                    _windowPath = null;
                }

                // Fonts are owned by the form (the controls only borrow them).
                if (_captionFont != null) { _captionFont.Dispose(); _captionFont = null; }
                if (_messageFont != null) { _messageFont.Dispose(); _messageFont = null; }
                if (_detailsFont != null) { _detailsFont.Dispose(); _detailsFont = null; }
                if (_buttonFont != null) { _buttonFont.Dispose(); _buttonFont = null; }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Layout

        private void UpdateDialogSize()
        {
            int width = Math.Max(
                Scale(MinWidth),
                Math.Min(Scale(MaxWidth), Scale(DesignWidth)));

            int contentWidth = width - Scale(OuterPadding) * 2;

            int textWidth = Math.Max(
                Scale(220),
                contentWidth - Scale(IconColumnWidth));

            _lblCaption.MaximumSize = new Size(textWidth, 0);
            _lblMessage.MaximumSize = new Size(textWidth, 0);

            // Measure uncapped first.
            _messagePanel.MaximumSize = Size.Empty;

            _rootLayout.SuspendLayout();

            Size preferred;

            try
            {
                _rootLayout.PerformLayout();

                preferred = _rootLayout.GetPreferredSize(
                    new Size(width - Scale(2), 0));
            }
            finally
            {
                _rootLayout.ResumeLayout(true);
            }

            int chrome = Scale(AccentBarHeight) + Scale(2);

            int maxAllowed;

            try
            {
                maxAllowed = Math.Min(
                    Scale(MaxHeight),
                    Screen.FromControl(this).WorkingArea.Height -
                        Scale(40));
            }
            catch
            {
                maxAllowed = Scale(MaxHeight);
            }

            int requiredHeight = preferred.Height + chrome;

            if (requiredHeight > maxAllowed)
            {
                // Too tall: cap the message area -> it starts scrolling
                // instead of clipping content off the window.
                int overflow = requiredHeight - maxAllowed;

                int capped = Math.Max(
                    Scale(96),
                    _messagePanel.Height - overflow);

                _messagePanel.MaximumSize = new Size(0, capped);

                _rootLayout.SuspendLayout();

                try
                {
                    _rootLayout.PerformLayout();

                    preferred = _rootLayout.GetPreferredSize(
                        new Size(width - Scale(2), 0));
                }
                finally
                {
                    _rootLayout.ResumeLayout(true);
                }

                requiredHeight = Math.Min(
                    preferred.Height + chrome,
                    maxAllowed);
            }

            requiredHeight = Math.Max(requiredHeight, Scale(MinHeight));
            requiredHeight = Math.Min(requiredHeight, maxAllowed);

            ClientSize = new Size(width, requiredHeight);

            try
            {
                MaximumSize = new Size(
                    Scale(MaxWidth),
                    Screen.FromControl(this).WorkingArea.Height);
            }
            catch
            {
                MaximumSize = new Size(
                    Scale(MaxWidth),
                    Scale(MaxHeight));
            }
        }

        private void UpdateWindowRegion()
        {
            if (_nativeRounding)
            {
                return; // DWM owns the corners.
            }

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            GraphicsPath newPath =
                GraphicsHelpers.CreateRoundedRectanglePath(
                    new Rectangle(0, 0, Width, Height),
                    Scale(CornerRadius));

            GraphicsPath oldPath = _windowPath;
            _windowPath = newPath;

            Region oldRegion = Region;
            Region = new Region(newPath);

            if (oldPath != null)
            {
                oldPath.Dispose();
            }

            if (oldRegion != null)
            {
                oldRegion.Dispose();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // HighQuality pixel offset is the difference between jagged
            // and clean rounded corners on GDI+ arcs.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality =
                CompositingQuality.HighQuality;

            if (_windowPath == null)
            {
                using (SolidBrush fallback =
                    new SolidBrush(_palette.Surface))
                {
                    e.Graphics.FillRectangle(
                        fallback,
                        ClientRectangle);
                }

                return;
            }

            using (SolidBrush brush =
                new SolidBrush(_palette.Surface))
            {
                e.Graphics.FillPath(brush, _windowPath);
            }

            if (_borderPen != null)
            {
                // Inset by half the pen width so the full border stays
                // inside the clipping region.
                float inset = _borderPen.Width / 2f;

                using (GraphicsPath borderPath =
                    GraphicsHelpers.CreateRoundedRectanglePath(
                        new RectangleF(
                            inset,
                            inset,
                            Width - _borderPen.Width,
                            Height - _borderPen.Width),
                        Math.Max(
                            2,
                            Scale(CornerRadius) - (int)Math.Ceiling(inset))))
                {
                    e.Graphics.DrawPath(_borderPen, borderPath);
                }
            }
        }

        #endregion

        #region Owner Positioning

        private void CenterOnOwner()
        {
            Rectangle target;

            IWin32Window owner = Owner;

            if (owner is Control)
            {
                Control control = (Control)owner;

                target = control.IsDisposed
                    ? Screen.PrimaryScreen.WorkingArea
                    : control.RectangleToScreen(control.ClientRectangle);
            }
            else if (owner != null)
            {
                try
                {
                    target = Screen.FromHandle(owner.Handle)
                        .WorkingArea;
                }
                catch
                {
                    target = Screen.PrimaryScreen.WorkingArea;
                }
            }
            else
            {
                target = Screen.PrimaryScreen.WorkingArea;
            }

            Screen screen = Screen.FromRectangle(target);
            Rectangle workingArea = screen.WorkingArea;

            int x = target.Left + ((target.Width - Width) / 2);
            int y = target.Top + ((target.Height - Height) / 2);

            Location = ClampToArea(new Point(x, y), workingArea);
        }

        private void ClampToWorkingArea()
        {
            Rectangle workingArea =
                Screen.FromControl(this).WorkingArea;

            Location = ClampToArea(Location, workingArea);
        }

        private Point ClampToArea(Point location, Rectangle area)
        {
            int x = location.X;
            int y = location.Y;

            if (x < area.Left) x = area.Left;
            if (y < area.Top) y = area.Top;
            if (x + Width > area.Right) x = area.Right - Width;
            if (y + Height > area.Bottom) y = area.Bottom - Height;

            return new Point(x, y);
        }

        #endregion

        #region Details Border

        private void detailsContainer_Paint(
            object sender,
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (GraphicsPath path =
                GraphicsHelpers.CreateRoundedRectanglePath(
                    new RectangleF(
                        0.5f,
                        0.5f,
                        _detailsContainer.Width - 1.5f,
                        _detailsContainer.Height - 1.5f),
                    Scale(6)))
            using (Pen pen = new Pen(_palette.Border, 1f))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        #endregion

        #region Native Shadow

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                if (!SystemInformation.HighContrast)
                {
                    cp.ClassStyle |= CS_DROPSHADOW;
                }

                return cp;
            }
        }

        #endregion
    }

    #endregion

    #region Message Icon

    internal sealed class HMessageIconControl : Control
    {
        private HMessageType _messageType;
        private Color _accentColor;
        private Color _surfaceColor;

        private HMessagePalette _palette;

        private Pen _glyphPen;
        private Pen _ringPen;
        private SolidBrush _glyphBrush;
        private SolidBrush _backgroundBrush;
        private SolidBrush _innerBrush;
        private Font _questionFont;
        private int _questionFontSize = -1;

        public HMessageIconControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            TabStop = false;

            _messageType = HMessageType.Information;
            _accentColor = Color.FromArgb(37, 118, 208);
            _surfaceColor = Color.White;

            UpdateResources();
        }

        public HMessagePalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value;

                _surfaceColor = value != null
                    ? value.Surface
                    : Color.White;

                UpdateResources();
                Invalidate();
            }
        }

        public HMessageType MessageType
        {
            get { return _messageType; }
            set
            {
                if (_messageType == value)
                {
                    return;
                }

                _messageType = value;
                Invalidate();
            }
        }

        public Color AccentColor
        {
            get { return _accentColor; }
            set
            {
                if (_accentColor == value)
                {
                    return;
                }

                _accentColor = value;

                UpdateResources();
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            UpdateResources();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality =
                CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint =
                TextRenderingHint.ClearTypeGridFit;

            int size = Math.Min(
                ClientSize.Width,
                ClientSize.Height);

            if (size <= 2)
            {
                return;
            }

            int x = (ClientSize.Width - size) / 2;
            int y = (ClientSize.Height - size) / 2;

            Rectangle circle = new Rectangle(
                x, y, size - 1, size - 1);

            if (_backgroundBrush == null)
            {
                UpdateResources();
            }

            e.Graphics.FillEllipse(_backgroundBrush, circle);

            // Subtle accent ring -> gives the icon a finished, colored look.
            if (_ringPen != null)
            {
                float inset = _ringPen.Width / 2f + 0.5f;

                e.Graphics.DrawEllipse(
                    _ringPen,
                    circle.X + inset,
                    circle.Y + inset,
                    circle.Width - inset * 2,
                    circle.Height - inset * 2);
            }

            switch (_messageType)
            {
                case HMessageType.Information:
                    DrawInformation(e.Graphics, circle);
                    break;

                case HMessageType.Success:
                    DrawSuccess(e.Graphics, circle);
                    break;

                case HMessageType.Warning:
                    DrawWarning(e.Graphics, circle);
                    break;

                case HMessageType.Error:
                    DrawError(e.Graphics, circle);
                    break;

                case HMessageType.Question:
                    DrawQuestion(e.Graphics, circle);
                    break;

                case HMessageType.Delete:
                    DrawDelete(e.Graphics, circle);
                    break;

                case HMessageType.Exit:
                    DrawExit(e.Graphics, circle);
                    break;

                case HMessageType.Hide:
                    DrawHide(e.Graphics, circle);
                    break;
            }
        }

        private void DrawInformation(Graphics graphics, Rectangle b)
        {
            float centerX = b.Left + b.Width / 2f;
            float dot = b.Width * 0.115f;

            graphics.FillEllipse(
                _glyphBrush,
                centerX - dot / 2f,
                b.Top + b.Height * 0.20f,
                dot,
                dot);

            float barWidth = b.Width * 0.115f;

            graphics.FillRectangle(
                _glyphBrush,
                centerX - barWidth / 2f,
                b.Top + b.Height * 0.37f,
                barWidth,
                b.Height * 0.40f);
        }

        private void DrawSuccess(Graphics graphics, Rectangle b)
        {
            PointF p1 = new PointF(
                b.Left + b.Width * 0.26f,
                b.Top + b.Height * 0.53f);

            PointF p2 = new PointF(
                b.Left + b.Width * 0.44f,
                b.Top + b.Height * 0.70f);

            PointF p3 = new PointF(
                b.Left + b.Width * 0.76f,
                b.Top + b.Height * 0.32f);

            graphics.DrawLines(_glyphPen, new[] { p1, p2, p3 });
        }

        private void DrawError(Graphics graphics, Rectangle b)
        {
            float left = b.Left + b.Width * 0.31f;
            float right = b.Left + b.Width * 0.69f;
            float top = b.Top + b.Height * 0.31f;
            float bottom = b.Top + b.Height * 0.69f;

            graphics.DrawLine(_glyphPen, left, top, right, bottom);
            graphics.DrawLine(_glyphPen, right, top, left, bottom);
        }

        private void DrawWarning(Graphics graphics, Rectangle b)
        {
            PointF top = new PointF(
                b.Left + b.Width / 2f,
                b.Top + b.Height * 0.20f);

            PointF left = new PointF(
                b.Left + b.Width * 0.24f,
                b.Top + b.Height * 0.78f);

            PointF right = new PointF(
                b.Left + b.Width * 0.76f,
                b.Top + b.Height * 0.78f);

            using (GraphicsPath triangle = new GraphicsPath())
            {
                triangle.AddPolygon(new[] { top, right, left });

                graphics.FillPath(_glyphBrush, triangle);
            }

            float centerX = b.Left + b.Width / 2f;
            float barWidth = Math.Max(2f, b.Width * 0.085f);

            graphics.FillRectangle(
                _innerBrush,
                centerX - barWidth / 2f,
                b.Top + b.Height * 0.38f,
                barWidth,
                b.Height * 0.22f);

            float dot = Math.Max(2f, b.Width * 0.085f);

            graphics.FillEllipse(
                _innerBrush,
                centerX - dot / 2f,
                b.Top + b.Height * 0.65f,
                dot,
                dot);
        }

        private void DrawQuestion(Graphics graphics, Rectangle b)
        {
            int wanted = Math.Max(8, (int)(b.Width * 0.52f));

            if (_questionFont == null || _questionFontSize != wanted)
            {
                if (_questionFont != null)
                {
                    _questionFont.Dispose();
                }

                _questionFont = new Font(
                    "Segoe UI",
                    wanted,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

                _questionFontSize = wanted;
            }

            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                graphics.DrawString(
                    "?",
                    _questionFont,
                    _glyphBrush,
                    b,
                    format);
            }
        }

        private void DrawDelete(Graphics graphics, Rectangle b)
        {
            float cx = b.Left + b.Width / 2f;
            float w = b.Width * 0.5f;
            float h = b.Height * 0.55f;
            float top = b.Top + b.Height * 0.25f;
            float left = cx - w / 2f;

            // Lid
            float lidWidth = w * 1.2f;
            float lidHeight = h * 0.15f;
            graphics.FillRectangle(_glyphBrush, cx - lidWidth / 2f, top - lidHeight, lidWidth, lidHeight);

            // Body (trapezoid)
            PointF[] body = new PointF[]
            {
        new PointF(left, top),
        new PointF(left + w, top),
        new PointF(left + w * 0.85f, top + h),
        new PointF(left + w * 0.15f, top + h)
            };
            graphics.FillPolygon(_glyphBrush, body);

            // Inner lines
            float lineWidth = Math.Max(1.5f, b.Width * 0.04f);
            using (Pen pen = new Pen(_surfaceColor, lineWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                float lineTop = top + h * 0.2f;
                float lineBottom = top + h * 0.8f;
                graphics.DrawLine(pen, cx - w * 0.15f, lineTop, cx - w * 0.15f, lineBottom);
                graphics.DrawLine(pen, cx + w * 0.15f, lineTop, cx + w * 0.15f, lineBottom);
            }
        }

        private void DrawExit(Graphics graphics, Rectangle b)
        {
            float cx = b.Left + b.Width / 2f;
            float cy = b.Top + b.Height / 2f;
            float radius = b.Width * 0.35f;
            float thickness = Math.Max(2f, b.Width * 0.12f);

            using (Pen pen = new Pen(_glyphBrush, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                // Arc (power symbol circle part, open at the top)
                graphics.DrawArc(pen, cx - radius, cy - radius, radius * 2, radius * 2, 30, 300);

                // Vertical line in the gap
                float lineTop = cy - radius * 0.9f;
                float lineBottom = cy;
                graphics.DrawLine(pen, cx, lineTop, cx, lineBottom);
            }
        }

        private void DrawHide(Graphics graphics, Rectangle b)
        {
            float cx = b.Left + b.Width / 2f;
            float cy = b.Top + b.Height / 2f;
            float w = b.Width * 0.6f;
            float h = b.Height * 0.35f;
            float penWidth = Math.Max(2f, b.Width * 0.08f);

            using (Pen pen = new Pen(_glyphBrush, penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                // Eye outline
                using (GraphicsPath eye = new GraphicsPath())
                {
                    eye.AddBezier(cx - w / 2f, cy, cx - w / 4f, cy - h, cx + w / 4f, cy - h, cx + w / 2f, cy);
                    eye.AddBezier(cx + w / 2f, cy, cx + w / 4f, cy + h, cx - w / 4f, cy + h, cx - w / 2f, cy);
                    graphics.DrawPath(pen, eye);
                }

                // Pupil
                float pupilR = b.Width * 0.1f;
                graphics.FillEllipse(_glyphBrush, cx - pupilR, cy - pupilR, pupilR * 2, pupilR * 2);

                // Slash
                float pad = b.Width * 0.15f;
                graphics.DrawLine(pen, cx - w / 2f - pad, cy + h + pad, cx + w / 2f + pad, cy - h - pad);
            }
        }

        private void UpdateResources()
        {
            DisposeResources();

            Color background = GraphicsHelpers.WithAlpha(
                GraphicsHelpers.Blend(_surfaceColor, _accentColor, 0.70f),
                38);

            _backgroundBrush = new SolidBrush(background);
            _glyphBrush = new SolidBrush(_accentColor);
            _innerBrush = new SolidBrush(_surfaceColor);

            float glyphWidth = Math.Max(
                2f,
                ClientSize.Width / 14f);

            _glyphPen = new Pen(_accentColor, glyphWidth);
            _glyphPen.StartCap = LineCap.Round;
            _glyphPen.EndCap = LineCap.Round;

            float ringWidth = Math.Max(
                1.5f,
                ClientSize.Width / 24f);

            _ringPen = new Pen(
                GraphicsHelpers.WithAlpha(_accentColor, 72),
                ringWidth);
        }

        private void DisposeResources()
        {
            if (_glyphPen != null) { _glyphPen.Dispose(); _glyphPen = null; }
            if (_ringPen != null) { _ringPen.Dispose(); _ringPen = null; }
            if (_glyphBrush != null) { _glyphBrush.Dispose(); _glyphBrush = null; }
            if (_backgroundBrush != null) { _backgroundBrush.Dispose(); _backgroundBrush = null; }
            if (_innerBrush != null) { _innerBrush.Dispose(); _innerBrush = null; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResources();

                if (_questionFont != null)
                {
                    _questionFont.Dispose();
                    _questionFont = null;
                }
            }

            base.Dispose(disposing);
        }
    }

    #endregion

    #region Message Button

    internal enum HMessageButtonKind
    {
        Primary,
        Secondary,
        Link
    }

    internal sealed class HMessageButton : System.Windows.Forms.Button
    {
        private HMessageButtonKind _buttonKind;

        private bool _hover;
        private bool _pressed;

        private HMessagePalette _palette;
        private Color _accent = Color.FromArgb(77, 87, 201);

        private GraphicsPath _path;

        private SolidBrush _backgroundBrush;
        private SolidBrush _hoverBrush;
        private SolidBrush _pressedBrush;
        private SolidBrush _textBrush;
        private SolidBrush _disabledBackgroundBrush;
        private SolidBrush _disabledTextBrush;

        private Pen _borderPen;
        private Pen _focusPen;

        public HMessageButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);


            FlatStyle = FlatStyle.Flat;
            UseVisualStyleBackColor = false;
            TabStop = true;
            Cursor = Cursors.Hand;

            UpdateResources();
        }



        public HMessagePalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value;
                UpdateResources();
                Invalidate();
            }
        }

        public Color AccentColor
        {
            get { return _accent; }
            set
            {
                if (_accent == value)
                {
                    return;
                }

                _accent = value;

                if (_focusPen != null)
                {
                    _focusPen.Dispose();
                    _focusPen = null;
                }

                Invalidate();
            }
        }

        public HMessageButtonKind ButtonKind
        {
            get { return _buttonKind; }
            set
            {
                if (_buttonKind == value)
                {
                    return;
                }

                _buttonKind = value;

                UpdateResources();
                Invalidate();
            }
        }

        //protected override void OnSizeChanged(EventArgs e)
        //{
        //    base.OnSizeChanged(e);

        //    UpdatePath();
        //    Invalidate();
        //}
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            UpdatePath();
            UpdateRegion();

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hover = false;
            _pressed = false;

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _pressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (_buttonKind == HMessageButtonKind.Link)
            {
                DrawLinkButton(graphics);
                return;
            }

            if (_path == null)
                UpdatePath();

            if (_path == null)
                return;

            SolidBrush backgroundBrush;

            if (!Enabled)
            {
                backgroundBrush = _disabledBackgroundBrush;
            }
            else if (_pressed)
            {
                backgroundBrush = _pressedBrush;
            }
            else if (_hover)
            {
                backgroundBrush = _hoverBrush;
            }
            else
            {
                backgroundBrush = _backgroundBrush;
            }

            if (backgroundBrush != null)
                graphics.FillPath(backgroundBrush, _path);

            if (_borderPen != null)
                graphics.DrawPath(_borderPen, _path);

            Color textColor;

            if (Enabled)
            {
                textColor = _textBrush != null
                    ? _textBrush.Color
                    : ForeColor;
            }
            else
            {
                textColor = _disabledTextBrush != null
                    ? _disabledTextBrush.Color
                    : SystemColors.GrayText;
            }

            Rectangle textBounds = ClientRectangle;
            textBounds.Inflate(-4, 0);

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                textBounds,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);

            DrawFocusRing(graphics);
        }

        private void UpdateRegion()
        {
            if (Width <= 1 || Height <= 1)
            {
                Region = null;
                return;
            }

            using (GraphicsPath regionPath =
                GraphicsHelpers.CreateRoundedRectanglePath(
                    new RectangleF(
                        0,
                        0,
                        Width,
                        Height),
                    Math.Min(10f, Height / 4f)))
            {
                Region = new Region(regionPath);
            }
        }

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    //  base.OnPaint(e);

        //    Graphics graphics = e.Graphics;

        //    graphics.SmoothingMode = SmoothingMode.AntiAlias;
        //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //    graphics.CompositingQuality = CompositingQuality.HighQuality;

        //    if (_buttonKind == HMessageButtonKind.Link)
        //    {
        //        DrawLinkButton(e.Graphics);
        //        return;
        //    }

        //    if (_path == null)
        //    {
        //        UpdatePath();
        //    }

        //    if (_path == null)
        //    {
        //        return;
        //    }

        //    SolidBrush brush;

        //    if (!Enabled)
        //    {
        //        brush = _disabledBackgroundBrush;
        //    }
        //    else if (_pressed)
        //    {
        //        brush = _pressedBrush;
        //    }
        //    else if (_hover)
        //    {
        //        brush = _hoverBrush;
        //    }
        //    else
        //    {
        //        brush = _backgroundBrush;
        //    }

        //    if (brush != null)
        //    {
        //        e.Graphics.FillPath(brush, _path);
        //    }

        //    if (_borderPen != null)
        //    {
        //        e.Graphics.DrawPath(_borderPen, _path);
        //    }

        //    Color textColor = Enabled
        //        ? (_textBrush != null ? _textBrush.Color : ForeColor)
        //        : (_disabledTextBrush != null
        //            ? _disabledTextBrush.Color
        //            : SystemColors.GrayText);

        //    Rectangle textBounds = ClientRectangle;
        //    textBounds.Inflate(-4, 0);

        //    TextRenderer.DrawText(
        //        e.Graphics,
        //        Text,
        //        Font,
        //        textBounds,
        //        textColor,
        //        TextFormatFlags.HorizontalCenter |
        //        TextFormatFlags.VerticalCenter |
        //        TextFormatFlags.EndEllipsis |
        //        TextFormatFlags.NoPrefix);

        //    DrawFocusRing(e.Graphics);
        //}

        private void DrawLinkButton(Graphics graphics)
        {
            if (_palette == null)
            {
                return;
            }

            Color color = !Enabled
                ? _palette.DisabledText
                : (_hover ? _palette.LinkHover : _palette.Link);

            TextFormatFlags flags =
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding;

            if (RightToLeft == RightToLeft.Yes)
            {
                flags |= TextFormatFlags.Right |
                    TextFormatFlags.RightToLeft;
            }
            else
            {
                flags |= TextFormatFlags.Left;
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                ClientRectangle,
                color,
                flags);

            if (Focused && ShowFocusCues)
            {
                using (Pen pen = new Pen(
                    GraphicsHelpers.WithAlpha(_accent, 170), 1f))
                {
                    pen.DashStyle = DashStyle.Dot;

                    graphics.DrawRectangle(
                        pen,
                        0,
                        0,
                        Width - 1,
                        Height - 1);
                }
            }
        }

        private void DrawFocusRing(Graphics graphics)
        {
            if (!Focused || !ShowFocusCues)
            {
                return;
            }

            if (_focusPen == null)
            {
                _focusPen = new Pen(
                    GraphicsHelpers.WithAlpha(_accent, 170),
                    2f);
            }

            Rectangle focus = new Rectangle(
                3,
                3,
                Width - 7,
                Height - 7);

            if (focus.Width <= 0 || focus.Height <= 0)
            {
                return;
            }

            using (GraphicsPath focusPath =
                GraphicsHelpers.CreateRoundedRectanglePath(
                    focus,
                    Math.Max(4, Math.Min(10, Height / 4))))
            {
                graphics.DrawPath(_focusPen, focusPath);
            }
        }

        //private void UpdatePath()
        //{
        //    if (Width <= 1 || Height <= 1)
        //    {
        //        return;
        //    }

        //    if (_path != null)
        //    {
        //        _path.Dispose();
        //    }

        //    // 0.5px inset: the centered 1px border pen then covers whole
        //    // pixels instead of leaving a half-clipped, dirty edge.
        //    _path = GraphicsHelpers.CreateRoundedRectanglePath(
        //        new RectangleF( 0.5f, 0.5f, Width - 1f, Height - 1f),
        //        Math.Min(10f, Height / 4f));
        //}
        private void UpdatePath()
        {
            if (_path != null)
            {
                _path.Dispose();
                _path = null;
            }

            if (Width <= 1 || Height <= 1)
                return;

            float radius = Math.Min(10f, Height / 4f);

            _path = GraphicsHelpers.CreateRoundedRectanglePath(
                new RectangleF(
                    0.5f,
                    0.5f,
                    Math.Max(1f, Width - 1f),
                    Math.Max(1f, Height - 1f)),
                radius);
        }

        private void UpdateResources()
        {
            DisposeBrushes();

            HMessagePalette palette =
                _palette ?? HMessagePalette.CreateLight();

            if (_buttonKind == HMessageButtonKind.Primary)
            {
                _backgroundBrush =
                    new SolidBrush(palette.PrimaryButton);

                _hoverBrush =
                    new SolidBrush(palette.PrimaryButtonHover);

                _pressedBrush =
                    new SolidBrush(palette.PrimaryButtonPressed);

                _textBrush =
                    new SolidBrush(palette.PrimaryButtonText);

                _borderPen = new Pen(
                    GraphicsHelpers.Blend(
                        palette.PrimaryButton,
                        Color.Black,
                        0.12f),
                    1f);
            }
            else
            {
                _backgroundBrush =
                    new SolidBrush(palette.SecondaryButton);

                _hoverBrush =
                    new SolidBrush(palette.SecondaryButtonHover);

                _pressedBrush =
                    new SolidBrush(palette.SecondaryButtonPressed);

                _textBrush =
                    new SolidBrush(palette.SecondaryButtonText);

                _borderPen =
                    new Pen(palette.SecondaryButtonBorder, 1f);
            }

            _disabledBackgroundBrush =
                new SolidBrush(palette.DisabledBackground);

            _disabledTextBrush =
                new SolidBrush(palette.DisabledText);
        }

        private void DisposeBrushes()
        {
            if (_backgroundBrush != null) { _backgroundBrush.Dispose(); _backgroundBrush = null; }
            if (_hoverBrush != null) { _hoverBrush.Dispose(); _hoverBrush = null; }
            if (_pressedBrush != null) { _pressedBrush.Dispose(); _pressedBrush = null; }
            if (_textBrush != null) { _textBrush.Dispose(); _textBrush = null; }
            if (_disabledBackgroundBrush != null) { _disabledBackgroundBrush.Dispose(); _disabledBackgroundBrush = null; }
            if (_disabledTextBrush != null) { _disabledTextBrush.Dispose(); _disabledTextBrush = null; }
            if (_borderPen != null) { _borderPen.Dispose(); _borderPen = null; }
            if (_focusPen != null) { _focusPen.Dispose(); _focusPen = null; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_path != null)
                {
                    _path.Dispose();
                    _path = null;
                }

                DisposeBrushes();
            }

            base.Dispose(disposing);
        }
    }

    #endregion

    #region Buffered Layout Controls (flicker-free)

    internal class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }

    internal class BufferedTableLayoutPanel : TableLayoutPanel
    {
        public BufferedTableLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }

    internal class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public BufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }

    #endregion

    #region Graphics Helpers

    internal static class GraphicsHelpers
    {
        public static GraphicsPath CreateRoundedRectanglePath(
            Rectangle rectangle,
            int radius)
        {
            return CreateRoundedRectanglePath(
                new RectangleF(
                    rectangle.X,
                    rectangle.Y,
                    rectangle.Width,
                    rectangle.Height),
                radius);
        }

        public static GraphicsPath CreateRoundedRectanglePath(
            RectangleF rectangle,
            float radius)
        {
            float diameter = radius * 2f;

            if (diameter > rectangle.Width)
            {
                diameter = rectangle.Width;
            }

            if (diameter > rectangle.Height)
            {
                diameter = rectangle.Height;
            }

            if (diameter < 1f)
            {
                diameter = 1f;
            }

            GraphicsPath path = new GraphicsPath();

            RectangleF arc = new RectangleF(
                rectangle.Left,
                rectangle.Top,
                diameter,
                diameter);

            path.AddArc(arc, 180, 90);

            arc.X = rectangle.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = rectangle.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = rectangle.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            return path;
        }

        public static Color Blend(
            Color first,
            Color second,
            float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));

            int r = (int)(first.R + ((second.R - first.R) * amount));
            int g = (int)(first.G + ((second.G - first.G) * amount));
            int b = (int)(first.B + ((second.B - first.B) * amount));

            return Color.FromArgb(r, g, b);
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, alpha)),
                color);
        }
    }

    #endregion

    #region Native Methods

    internal static class NativeMethods
    {
        public const uint SPI_GETCLIENTAREAANIMATION = 0x1042;

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            [MarshalAs(UnmanagedType.Bool)] ref bool pvParam,
            uint fWinIni);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        /// <summary>
        /// Asks the DWM for native rounded corners (Windows 11+).
        /// Returns false on older systems so the caller can fall back
        /// to a Region-based shape.
        /// </summary>
        public static bool TryEnableRoundedCorners(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                int preference = DWMWCP_ROUND;

                int hr = DwmSetWindowAttribute(
                    handle,
                    DWMWA_WINDOW_CORNER_PREFERENCE,
                    ref preference,
                    Marshal.SizeOf(typeof(int)));

                return hr == 0; // S_OK
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
        }
    }

    #endregion
}

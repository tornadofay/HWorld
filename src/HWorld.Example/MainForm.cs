using System;
using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.World;
using HWorld.Core.Geometry;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    internal sealed class MainForm : Form
    {
        private readonly WorldCanvas _canvas;
        private readonly Timer _timer;

        private Label _timeValue;
        private Label _itemsValue;
        private Label _statusValue;
        private Label _zoomValue;
        private TextBox _seedBox;
        private ComboBox _toolBox;
        private CheckBox _solidBox;
        private Label _modeValue;
        private Label _playerValue;
        private Label _seedValue;
        private Label _storyValue;
        private Label _hintValue;

        private WorldScenario _scenario;
        private bool _running;
        private bool _up;
        private bool _down;
        private bool _left;
        private bool _right;

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld",
                Subtitle = "World playground  •  Build, explore, and experiment",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate
            {
                HMessage.ShowInformation(this,
                    "Build mode: click to place objects and right-click to remove them.\r\nPlay mode: use WASD or the arrow keys.\r\nMouse wheel: zoom. Middle mouse: pan.",
                    "HWorld controls");
            };
            root.Controls.Add(header, 0, 0);
            root.SetColumnSpan(header, 2);

            root.Controls.Add(BuildSidebar(), 0, 1);

            _canvas = new WorldCanvas
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 0, 0, 0),
                Mode = CanvasMode.Observe
            };
            root.Controls.Add(_canvas, 1, 1);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            FormClosing += delegate { _timer.Stop(); };
            _canvas.WorldEdited += delegate { UpdateStatus(); };

            LoadHandBuiltWorld();
        }

        private Control BuildSidebar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 25, 32),
                Padding = new Padding(16)
            };

            var title = MakeLabel("WORLD LAB", 12f, FontStyle.Bold, Color.FromArgb(246, 248, 250));
            title.Dock = DockStyle.Top;
            title.Height = 26;
            panel.Controls.Add(title);

            var source = MakeLabel("Create your world or generate one from a seed.", 8.7f, FontStyle.Regular, Color.FromArgb(146, 158, 171));
            source.Dock = DockStyle.Top;
            source.Height = 38;
            panel.Controls.Add(source);

            var hand = MakeButton("Hand-built world");
            hand.Click += delegate { LoadHandBuiltWorld(); };
            panel.Controls.Add(hand);

            var seedRow = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 5, 0, 5) };
            panel.Controls.Add(seedRow);
            _seedBox = new TextBox
            {
                Text = "20260830",
                Dock = DockStyle.Left,
                Width = 145,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(12, 16, 21),
                ForeColor = Color.FromArgb(230, 234, 239),
                BorderStyle = BorderStyle.FixedSingle
            };
            seedRow.Controls.Add(_seedBox);

            var random = MakeButton("Generate seed");
            random.Width = 110;
            random.Height = 34;
            random.Location = new Point(153, 5);
            random.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            random.Click += delegate { GenerateSeededWorld(); };
            seedRow.Controls.Add(random);

            panel.Controls.Add(MakeSectionLabel("MODE"));
            var build = MakeButton("Build mode");
            build.Click += delegate { SetMode(CanvasMode.Build); };
            panel.Controls.Add(build);
            var play = MakeButton("Play as me");
            play.Click += delegate { SetMode(CanvasMode.Play); };
            panel.Controls.Add(play);
            var observe = MakeButton("Observe");
            observe.Click += delegate { SetMode(CanvasMode.Observe); };
            panel.Controls.Add(observe);

            panel.Controls.Add(MakeSectionLabel("BUILD TOOL"));
            _toolBox = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 34,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(12, 16, 21),
                ForeColor = Color.FromArgb(230, 234, 239),
                FlatStyle = FlatStyle.Flat
            };
            _toolBox.Items.Add("object");
            _toolBox.Items.Add("wall");
            _toolBox.Items.Add("nature");
            _toolBox.Items.Add("resource");
            _toolBox.Items.Add("landmark");
            _toolBox.SelectedIndex = 0;
            _toolBox.SelectedIndexChanged += delegate { ApplyTool(); };
            panel.Controls.Add(_toolBox);

            _solidBox = new CheckBox
            {
                Text = "Solid / collidable",
                Dock = DockStyle.Top,
                Height = 32,
                Checked = false,
                ForeColor = Color.FromArgb(205, 213, 222),
                BackColor = Color.Transparent
            };
            _solidBox.CheckedChanged += delegate { ApplyTool(); };
            panel.Controls.Add(_solidBox);

            var fit = MakeButton("Center / fit world");
            fit.Click += delegate { _canvas.ResetView(); };
            panel.Controls.Add(fit);

            panel.Controls.Add(MakeSectionLabel("WORLD"));
            _modeValue = AddMetric(panel, "Mode");
            _timeValue = AddMetric(panel, "Simulation time");
            _itemsValue = AddMetric(panel, "Objects");
            _playerValue = AddMetric(panel, "Player");
            _seedValue = AddMetric(panel, "Seed");
            _zoomValue = AddMetric(panel, "Zoom");
            _statusValue = AddMetric(panel, "Status");

            panel.Controls.Add(MakeSectionLabel("STORY"));
            _storyValue = MakeLabel("—", 8.5f, FontStyle.Regular, Color.FromArgb(173, 182, 193));
            _storyValue.Dock = DockStyle.Top;
            _storyValue.Height = 72;
            _storyValue.AutoEllipsis = false;
            panel.Controls.Add(_storyValue);

            _hintValue = MakeLabel("", 8.2f, FontStyle.Regular, Color.FromArgb(119, 132, 145));
            _hintValue.Dock = DockStyle.Fill;
            panel.Controls.Add(_hintValue);
            return panel;
        }

        private HButton MakeButton(string text)
        {
            return new HButton
            {
                Text = text,
                Width = 250,
                Height = 36,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 4),
                ButtonLeaveBackGroundColor1 = Color.FromArgb(47, 57, 69),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(30, 37, 46),
                ButtonEnterBackGroundColor1 = Color.FromArgb(73, 88, 105),
                ButtonEnterBackGroundColor2 = Color.FromArgb(47, 57, 69),
                ButtonDownBackGroundColor1 = Color.FromArgb(33, 40, 49),
                ButtonDownBackGroundColor2 = Color.FromArgb(25, 31, 38),
                ButtonLeaveForeColor = Color.FromArgb(231, 237, 244),
                ButtonEnterForeColor = Color.White,
                ButtonDownForeColor = Color.White
            };
        }

        private Label MakeSectionLabel(string text)
        {
            var label = MakeLabel(text, 8f, FontStyle.Bold, Color.FromArgb(104, 193, 228));
            label.Dock = DockStyle.Top;
            label.Height = 24;
            label.Padding = new Padding(0, 10, 0, 0);
            return label;
        }

        private Label AddMetric(Control parent, string caption)
        {
            var row = new Panel { Dock = DockStyle.Top, Height = 35 };
            parent.Controls.Add(row);
            parent.Controls.SetChildIndex(row, 0);
            var label = MakeLabel(caption, 7.8f, FontStyle.Regular, Color.FromArgb(126, 139, 153));
            label.Dock = DockStyle.Top;
            label.Height = 15;
            row.Controls.Add(label);
            var value = MakeLabel("—", 9.5f, FontStyle.Bold, Color.FromArgb(225, 231, 237));
            value.Dock = DockStyle.Fill;
            row.Controls.Add(value);
            return value;
        }

        private static Label MakeLabel(string text, float size, FontStyle style, Color color)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, BackColor = Color.Transparent, AutoEllipsis = true };
        }

        private void LoadHandBuiltWorld()
        {
            _timer.Stop();
            _running = false;
            _scenario = WorldScenarioFactory.CreateHandBuilt();
            ApplyScenario();
            SetMode(CanvasMode.Build);
        }

        private void GenerateSeededWorld()
        {
            int seed;
            if (!int.TryParse(_seedBox.Text.Trim(), out seed))
            {
                HMessage.ShowWarning(this, "Enter a valid integer seed.", "Generate world");
                _seedBox.Focus();
                return;
            }

            _timer.Stop();
            _running = false;
            _scenario = WorldScenarioFactory.CreateSeeded(seed);
            ApplyScenario();
            SetMode(CanvasMode.Build);
        }

        private void ApplyScenario()
        {
            _canvas.World = _scenario.World;
            _canvas.Player = _scenario.Player;
            _canvas.Mode = CanvasMode.Build;
            _canvas.ResetView();
            _storyValue.Text = _scenario.Story;
            ApplyTool();
            UpdateStatus();
        }

        private void SetMode(CanvasMode mode)
        {
            _canvas.Mode = mode;
            if (mode == CanvasMode.Play)
            {
                _running = true;
                _timer.Start();
                _canvas.Focus();
                _canvas.CenterOnPlayer();
                _hintValue.Text = "PLAY  •  WASD / arrow keys to move  •  mouse wheel to zoom";
            }
            else if (mode == CanvasMode.Build)
            {
                _running = false;
                _timer.Stop();
                _canvas.Focus();
                _hintValue.Text = "BUILD  •  click to place  •  right-click to remove  •  wheel to zoom  •  middle drag to pan";
            }
            else
            {
                _running = false;
                _timer.Stop();
                _hintValue.Text = "OBSERVE  •  the simulation is paused";
            }
            UpdateStatus();
            _canvas.Invalidate();
        }

        private void ApplyTool()
        {
            if (_toolBox == null || _solidBox == null || _canvas == null) return;
            var kind = _toolBox.SelectedItem as string ?? "object";
            _canvas.BuildKind = kind;
            _canvas.BuildSolid = _solidBox.Checked || kind == "wall";
            if (kind == "wall") { _canvas.BuildWidth = 14; _canvas.BuildHeight = 5; }
            else { _canvas.BuildWidth = 8; _canvas.BuildHeight = 8; }
        }

        private void OnTick(object sender, EventArgs e)
        {
            const double dt = 1.0 / 30.0;
            if (!_running || _scenario == null) return;
            double x = 0, y = 0;
            if (_left) x -= 1; if (_right) x += 1; if (_up) y -= 1; if (_down) y += 1;
            if (Math.Abs(x) > 0 || Math.Abs(y) > 0)
                _scenario.World.MoveActor(_scenario.Player.Id, x, y, dt);
            _scenario.World.Update(dt);
            UpdateStatus();
            _canvas.Invalidate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = true;
            if (_canvas.Mode == CanvasMode.Play && e.KeyCode == Keys.Escape) SetMode(CanvasMode.Observe);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = false;
        }

        private void UpdateStatus()
        {
            if (_scenario == null) return;
            _modeValue.Text = _canvas.Mode.ToString();
            _timeValue.Text = _scenario.World.SimulationTime.ToString("0.00") + " s";
            _itemsValue.Text = _scenario.World.Items.Count.ToString();
            _playerValue.Text = string.Format("{0:0.0}, {1:0.0}", _scenario.Player.Position.X, _scenario.Player.Position.Y);
            _seedValue.Text = _scenario.Seed.HasValue ? _scenario.Seed.Value.ToString() : "manual";
            _zoomValue.Text = _canvas.Zoom.ToString("0.00") + "x";
            _statusValue.Text = _running ? "Running" : "Paused";
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    internal sealed class MainForm : Form
    {
        private readonly WorldCanvas _canvas;
        private readonly Label _timeValue;
        private readonly Label _itemsValue;
        private readonly Label _statusValue;
        private readonly Label _zoomValue;
        private readonly Timer _timer;

        private World _world;
        private bool _running;

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 620);
            ClientSize = new Size(1180, 720);
            BackColor = Color.FromArgb(12, 15, 19);
            ForeColor = Color.FromArgb(232, 236, 240);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = BackColor,
                Padding = new Padding(12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld Example",
                Subtitle = "2D simulation laboratory  •  GDI+ renderer",
                AllowMove = true,
                AllowMinimize = true,
                AllowClose = true
            };
            header.PerformOnClose += delegate { Close(); };
            root.Controls.Add(header, 0, 0);
            root.SetColumnSpan(header, 2);

            var sidebar = BuildSidebar();
            root.Controls.Add(sidebar, 0, 1);

            _canvas = new WorldCanvas
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 0, 0)
            };
            root.Controls.Add(_canvas, 1, 1);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += OnTick;

            CreateWorld();
            UpdateStatus();
        }

        private Control BuildSidebar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(21, 26, 33),
                Padding = new Padding(18)
            };

            var title = MakeLabel("Simulation", 13f, FontStyle.Bold, Color.FromArgb(245, 248, 250));
            title.Dock = DockStyle.Top;
            title.Height = 28;
            panel.Controls.Add(title);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 178,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(actions);

            var runButton = MakeHButton("Run", Color.FromArgb(44, 134, 89));
            runButton.Click += delegate
            {
                _running = true;
                _timer.Start();
                _statusValue.Text = "Running";
            };
            actions.Controls.Add(runButton);

            var pauseButton = MakeHButton("Pause", Color.FromArgb(148, 100, 43));
            pauseButton.Click += delegate
            {
                _running = false;
                _timer.Stop();
                _statusValue.Text = "Paused";
            };
            actions.Controls.Add(pauseButton);

            var stepButton = MakeHButton("Step", Color.FromArgb(49, 88, 124));
            stepButton.Click += delegate
            {
                _world.Update(1.0 / 30.0);
                UpdateStatus();
                _canvas.Invalidate();
            };
            actions.Controls.Add(stepButton);

            var resetButton = MakeHButton("Reset world", Color.FromArgb(73, 78, 87));
            resetButton.Click += delegate
            {
                if (HMessage.ShowQuestion(this, "Reset the current simulation?", "Reset world") != DialogResult.Yes)
                    return;

                _timer.Stop();
                _running = false;
                CreateWorld();
                _statusValue.Text = "Ready";
            };
            actions.Controls.Add(resetButton);

            var viewTitle = MakeLabel("View", 13f, FontStyle.Bold, Color.FromArgb(245, 248, 250));
            viewTitle.Dock = DockStyle.Top;
            viewTitle.Height = 28;
            panel.Controls.Add(viewTitle);

            var fitButton = MakeHButton("Fit world", Color.FromArgb(62, 70, 82));
            fitButton.Dock = DockStyle.Top;
            fitButton.Margin = new Padding(0, 6, 0, 8);
            fitButton.Click += delegate
            {
                _canvas.ResetView();
                UpdateStatus();
            };
            panel.Controls.Add(fitButton);

            var metrics = MakeLabel("World", 13f, FontStyle.Bold, Color.FromArgb(245, 248, 250));
            metrics.Dock = DockStyle.Top;
            metrics.Height = 28;
            panel.Controls.Add(metrics);

            _timeValue = AddMetric(panel, "Simulation time");
            _itemsValue = AddMetric(panel, "Items");
            _zoomValue = AddMetric(panel, "Zoom");
            _statusValue = AddMetric(panel, "Status");

            var help = MakeLabel(
                "Mouse wheel: zoom\r\nMiddle/right drag: pan\r\n\r\nThis example is intentionally independent from HAgent. It exercises the HWorld world core and WinForms/GDI+ presentation layer.",
                8.8f,
                FontStyle.Regular,
                Color.FromArgb(145, 157, 169));
            help.Dock = DockStyle.Bottom;
            help.Height = 108;
            panel.Controls.Add(help);

            return panel;
        }

        private static HButton MakeHButton(string text, Color background)
        {
            var button = new HButton
            {
                Text = text,
                Width = 214,
                Height = 36,
                Margin = new Padding(0, 0, 0, 8),
                ButtonLeaveBackGroundColor1 = background,
                ButtonLeaveBackGroundColor2 = background,
                ButtonEnterBackGroundColor1 = ControlPaint.Light(background, 0.12f),
                ButtonEnterBackGroundColor2 = background,
                ButtonDownBackGroundColor1 = ControlPaint.Dark(background, 0.12f),
                ButtonDownBackGroundColor2 = background,
                ButtonLeaveForeColor = Color.White,
                ButtonEnterForeColor = Color.White,
                ButtonDownForeColor = Color.White
            };
            return button;
        }

        private Label AddMetric(Control parent, string caption)
        {
            var row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                Padding = new Padding(0, 3, 0, 3)
            };
            parent.Controls.Add(row);
            parent.Controls.SetChildIndex(row, 0);

            var label = MakeLabel(caption, 8.5f, FontStyle.Regular, Color.FromArgb(145, 157, 169));
            label.Dock = DockStyle.Top;
            label.Height = 17;
            row.Controls.Add(label);

            var value = MakeLabel("—", 11f, FontStyle.Bold, Color.FromArgb(230, 235, 239));
            value.Dock = DockStyle.Fill;
            row.Controls.Add(value);
            return value;
        }

        private static Label MakeLabel(string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
        }

        private void CreateWorld()
        {
            _world = new World(160, 100);
            _world.AddItem(new WorldPoint(18, 18), 8, 8);
            _world.AddItem(new WorldPoint(42, 24), 5, 14, solid: true);
            _world.AddItem(new WorldPoint(68, 17), 14, 6);
            _world.AddItem(new WorldPoint(92, 42), 7, 7, solid: true);
            _world.AddItem(new WorldPoint(112, 24), 9, 16);
            _world.AddItem(new WorldPoint(128, 67), 16, 5, solid: true);
            _world.AddItem(new WorldPoint(55, 72), 6, 9);
            _world.AddItem(new WorldPoint(22, 64), 10, 5, solid: true);
            _canvas.World = _world;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_running)
                return;

            _world.Update(_timer.Interval / 1000.0);
            UpdateStatus();
            _canvas.Invalidate();
        }

        private void UpdateStatus()
        {
            if (_world == null)
                return;

            _timeValue.Text = _world.SimulationTime.ToString("0.00") + " s";
            _itemsValue.Text = _world.Items.Count.ToString();
            _zoomValue.Text = _canvas.Zoom.ToString("0.00") + "x";
        }
    }
}

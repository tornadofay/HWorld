using System;
using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;
using HWorld.WinForms.Rendering;

namespace HWorld.WinForms
{
    public sealed class GdiWorldForm : Form
    {
        private readonly GdiWorldCanvas _canvas;
        private readonly GeometryCameraView _cameraView;
        private readonly Timer _timer;
        private readonly WorldActor _player;
        private bool _up, _down, _left, _right;
        private Label _timeValue, _positionValue, _sensorValue;
        private bool _running = true;
        private bool _sensorMode;

        public GdiWorldForm(World world, WorldActor player = null)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            _player = player ?? (world.Actors.Count > 0 ? world.Actors[0] : world.AddActor(new WorldPoint(20, 20), speed: 14));
            if (string.IsNullOrWhiteSpace(_player.Name)) _player.Name = "Player";

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackColor, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header { Dock = DockStyle.Fill, Title = "HWorld GDI", Subtitle = "GDI+ world runtime", AllowMove = false, AllowMinimize = true, AllowClose = true, AllowHelp = true };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "WASD / arrow keys move the player. E interacts with nearby interactable items. Mouse wheel zooms. Middle mouse pans in world view. F2 toggles the Geometry Eye.", "GDI World"); };
            root.Controls.Add(header, 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250f));
            root.Controls.Add(content, 0, 1);

            var viewport = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, Margin = new Padding(0, 8, 8, 0) };
            _canvas = new GdiWorldCanvas { Dock = DockStyle.Fill, World = world, Player = _player, Mode = CanvasMode.Play };
            _cameraView = new GeometryCameraView { Dock = DockStyle.Fill, World = world, Observer = _player, Visible = false };
            _cameraView.Camera.Range = 55;
            _cameraView.Camera.FieldOfViewDegrees = 90;
            viewport.Controls.Add(_canvas);
            viewport.Controls.Add(_cameraView);
            content.Controls.Add(viewport, 0, 0);
            content.Controls.Add(BuildInfoPanel(), 1, 0);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            FormClosing += delegate { _timer.Stop(); };
            _timer.Start();
            _canvas.CenterOnPlayer();
            _canvas.Focus();
        }

        private Control BuildInfoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 25, 32), Padding = new Padding(14), AutoScroll = true };
            panel.Controls.Add(new Label { Text = "RUNTIME", Dock = DockStyle.Top, Height = 26, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.White });

            var pause = MakeButton("Pause");
            pause.Click += delegate { _running = !_running; pause.Text = _running ? "Pause" : "Resume"; };
            panel.Controls.Add(pause);

            var center = MakeButton("Center on player");
            center.Click += delegate { _canvas.CenterOnPlayer(); _canvas.Focus(); };
            panel.Controls.Add(center);

            var sensor = MakeButton("Geometry Eye");
            sensor.Click += delegate { SetSensorMode(!_sensorMode); };
            panel.Controls.Add(sensor);

            var interact = MakeButton("Interact (E)");
            interact.Click += delegate { InteractWithNearbyItem(); _canvas.Focus(); };
            panel.Controls.Add(interact);

            AddTop(panel, MakeMetricLabel("Time"), out _timeValue);
            AddTop(panel, MakeMetricLabel("Position"), out _positionValue);
            AddTop(panel, MakeMetricLabel("Sensor"), out _sensorValue);
            return panel;
        }

        private void SetSensorMode(bool enabled)
        {
            _sensorMode = enabled;
            _cameraView.Visible = enabled;
            _canvas.Visible = !enabled;
            _sensorValue.Text = enabled ? "Geometry Eye | 90° / 55 range" : "World view";
            if (enabled) _cameraView.RefreshObservation();
            else _canvas.CenterOnPlayer();
            (enabled ? (Control)_cameraView : _canvas).Focus();
        }

        private void InteractWithNearbyItem()
        {
            var item = _canvas.World.FindNearestInteractable(_player.Position, 10.0);
            if (item == null) return;

            var result = WorldInteraction.TryInteract(_canvas.World, _player.Id, item.Id, 10.0);
            if (result == WorldInteractionResult.Succeeded)
            {
                HMessage.ShowInformation(this,
                    item.Name + "\r\n\r\nAction: " + item.InteractionLabel + "\r\nPosition: " +
                    item.Position.X.ToString("0.0") + ", " + item.Position.Y.ToString("0.0"),
                    "Interaction");
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_running) return;
            const double dt = 1.0 / 30.0;
            double x = 0, y = 0;
            if (_left) x -= 1; if (_right) x += 1; if (_up) y -= 1; if (_down) y += 1;
            if (Math.Abs(x) > 0 || Math.Abs(y) > 0) _canvas.World.MoveActor(_player.Id, x, y, dt);
            _canvas.World.Update(dt);
            _cameraView.RefreshObservation();
            _timeValue.Text = "Time\r\n" + _canvas.World.SimulationTime.ToString("0.00") + " s";
            _positionValue.Text = "Position\r\n" + _player.Position.X.ToString("0.0") + ", " + _player.Position.Y.ToString("0.0");
            if (_sensorMode) _sensorValue.Text = "Geometry Eye | " + _cameraView.Observations.Count + " detected";
            _canvas.Invalidate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = true;
            if (e.KeyCode == Keys.E) InteractWithNearbyItem();
            if (e.KeyCode == Keys.F2) SetSensorMode(!_sensorMode);
            if (e.KeyCode == Keys.Escape) Close();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = false;
        }

        private static HButton MakeButton(string text) { return new HButton { Text = text, Dock = DockStyle.Top, Height = 36, Margin = new Padding(0, 4, 0, 4) }; }
        private static Label MakeMetricLabel(string caption) { return new Label { Text = caption + "\r\n—", Dock = DockStyle.Top, Height = 44, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(185, 195, 205), BackColor = Color.Transparent }; }
        private static void AddTop(Control parent, Label value, out Label target) { parent.Controls.Add(value); target = value; }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;
using HWorld.WinForms.Rendering;

namespace HWorld.Example
{
    internal sealed class MultiActorLabForm : Form
    {
        private readonly World _world;
        private readonly WorldActor _actorA;
        private readonly WorldActor _actorB;
        private readonly GeometryCameraView _sensorA;
        private readonly GeometryCameraView _sensorB;
        private readonly MultiActorOverview _overview;
        private readonly Timer _timer;
        private readonly TextBox _textA;
        private readonly TextBox _textB;
        private readonly Label _status;

        public MultiActorLabForm()
        {
            _world = new World(180, 110);
            AddBoundary(_world, 2, 2, 176, 3);
            AddBoundary(_world, 2, 105, 176, 3);
            AddBoundary(_world, 2, 2, 3, 106);
            AddBoundary(_world, 175, 2, 3, 106);

            var wall = _world.AddItem(new WorldPoint(83, 38), 14, 34, true);
            wall.Kind = "wall";
            wall.Name = "Central wall";

            _actorA = _world.AddActor(new WorldPoint(35, 30), width: 5, height: 5, speed: 10);
            _actorA.Name = "Actor A";
            _actorA.RotationDegrees = 0;
            _actorA.Controller = new WanderController(0);

            _actorB = _world.AddActor(new WorldPoint(145, 78), width: 5, height: 5, speed: 7);
            _actorB.Name = "Actor B";
            _actorB.RotationDegrees = 180;
            _actorB.Controller = new WanderController(2);

            _sensorA = new GeometryCameraView { Dock = DockStyle.Fill };
            _sensorA.Camera.Range = 60;
            _sensorA.Camera.FieldOfViewDegrees = 100;
            _sensorA.Camera.IncludeActors = true;
            _sensorA.World = _world;
            _sensorA.Observer = _actorA;

            _sensorB = new GeometryCameraView { Dock = DockStyle.Fill };
            _sensorB.Camera.Range = 60;
            _sensorB.Camera.FieldOfViewDegrees = 100;
            _sensorB.Camera.IncludeActors = true;
            _sensorB.World = _world;
            _sensorB.Observer = _actorB;

            _overview = new MultiActorOverview { Dock = DockStyle.Fill, World = _world, ActorA = _actorA, ActorB = _actorB, Margin = new Padding(0, 8, 8, 0) };

            _textA = MakeObservationBox();
            _textB = MakeObservationBox();
            _status = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(175, 187, 200), Font = new Font("Segoe UI", 8.5f) };

            Text = "HWorld Multi-Actor Laboratory";
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10), BackColor = BackColor };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header { Dock = DockStyle.Fill, Title = "HWorld Multi-Actor Laboratory", Subtitle = "Independent actors • actor-specific controllers • actor-specific Geometry Eye sensors", AllowMove = false, AllowMinimize = true, AllowClose = true, AllowHelp = true };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "Two independent actors move through the same authoritative world. Each actor has its own controller and sensor instance. The observation text is the exact Core serializer output.", "Multi-Actor Laboratory"); };
            root.Controls.Add(header, 0, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            root.Controls.Add(body, 0, 1);
            body.Controls.Add(_overview, 0, 0);
            body.Controls.Add(BuildSensorsPanel(), 1, 0);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += OnTick;
            FormClosing += delegate { _timer.Stop(); };
            _timer.Start();
            RefreshSensors();
        }

        private Control BuildSensorsPanel()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = BackColor, Padding = new Padding(0, 8, 0, 0) };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 46f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 46f));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            panel.Controls.Add(BuildSensorRow("Actor A sensor", _sensorA, _textA), 0, 0);
            panel.Controls.Add(BuildSensorRow("Actor B sensor", _sensorB, _textB), 0, 1);
            panel.Controls.Add(_status, 0, 2);
            return panel;
        }

        private static Control BuildSensorRow(string title, GeometryCameraView view, TextBox text)
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.FromArgb(20, 25, 32), Margin = new Padding(0, 0, 0, 8) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));

            var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            left.Controls.Add(view);
            var label = new Label { Text = title, Dock = DockStyle.Top, Height = 24, ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            left.Controls.Add(label);
            row.Controls.Add(left, 0, 0);

            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            var textLabel = new Label { Text = "EXACT OBSERVATION TEXT", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(190, 200, 210), Font = new Font("Segoe UI", 8f, FontStyle.Bold) };
            right.Controls.Add(textLabel);
            text.Dock = DockStyle.Fill;
            right.Controls.Add(text);
            row.Controls.Add(right, 1, 0);
            return row;
        }

        private static TextBox MakeObservationBox()
        {
            return new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(13, 17, 22),
                ForeColor = Color.FromArgb(220, 228, 236),
                Font = new Font("Consolas", 8.5f)
            };
        }

        private void OnTick(object sender, EventArgs e)
        {
            const double dt = 1.0 / 30.0;
            _world.Update(dt);
            RefreshSensors();
            _overview.Invalidate();
        }

        private void RefreshSensors()
        {
            _sensorA.RefreshObservation();
            _sensorB.RefreshObservation();
            var observationsA = new System.Collections.Generic.List<WorldGeometryObservation>(_sensorA.Observations);
            var observationsB = new System.Collections.Generic.List<WorldGeometryObservation>(_sensorB.Observations);
            var textA = WorldGeometryObservationSerializer.Serialize(observationsA);
            var textB = WorldGeometryObservationSerializer.Serialize(observationsB);
            _textA.Text = textA;
            _textB.Text = textB;
            _status.Text = string.Format("Time {0:0.00}s   A ({1:0.0},{2:0.0}) [{3} obs / ~{4} tok]   B ({5:0.0},{6:0.0}) [{7} obs / ~{8} tok]",
                _world.SimulationTime,
                _actorA.Position.X, _actorA.Position.Y, _sensorA.Observations.Count, WorldObservationTokenEstimator.EstimateTokens(textA),
                _actorB.Position.X, _actorB.Position.Y, _sensorB.Observations.Count, WorldObservationTokenEstimator.EstimateTokens(textB));
        }

        private static void AddBoundary(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
        }
    }

    internal sealed class WanderController : IWorldActorController
    {
        private static readonly double[,] Directions =
        {
            { 1, 0 },
            { 0, 1 },
            { -1, 0 },
            { 0, -1 }
        };

        private int _directionIndex;

        public WanderController(int initialDirection)
        {
            _directionIndex = ((initialDirection % 4) + 4) % 4;
        }

        public void Update(WorldActorControllerContext context)
        {
            if (context.IsBusy) return;
            var dx = Directions[_directionIndex, 0];
            var dy = Directions[_directionIndex, 1];
            context.Move(dx, dy, 1.5);
            _directionIndex = (_directionIndex + 1) % 4;
        }
    }

    internal sealed class MultiActorOverview : Control
    {
        public World World { get; set; }
        public WorldActor ActorA { get; set; }
        public WorldActor ActorB { get; set; }

        public MultiActorOverview()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(10, 13, 17);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (World == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            var scaleX = (ClientSize.Width - 28f) / (float)World.Width;
            var scaleY = (ClientSize.Height - 42f) / (float)World.Height;
            var scale = Math.Max(0.2f, Math.Min(scaleX, scaleY));
            var ox = 14f;
            var oy = 28f;

            using (var ground = new SolidBrush(Color.FromArgb(18, 24, 30)))
            using (var border = new Pen(Color.FromArgb(90, 106, 122), 1.4f))
            using (var solid = new SolidBrush(Color.FromArgb(170, 166, 79, 92)))
            using (var empty = new SolidBrush(Color.FromArgb(100, 70, 130, 185)))
            using (var actorBrush = new SolidBrush(Color.FromArgb(245, 93, 196, 255)))
            using (var actorBrush2 = new SolidBrush(Color.FromArgb(245, 150, 220, 130)))
            using (var actorPen = new Pen(Color.White, 1.4f))
            using (var headingPen = new Pen(Color.White, 1.5f))
            using (var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var actorFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                var worldRect = new RectangleF(ox, oy, (float)World.Width * scale, (float)World.Height * scale);
                g.FillRectangle(ground, worldRect);
                g.DrawRectangle(border, worldRect.X, worldRect.Y, worldRect.Width, worldRect.Height);

                for (int i = 0; i < World.Items.Count; i++)
                {
                    var item = World.Items[i];
                    var rect = new RectangleF(
                        ox + (float)item.Position.X * scale,
                        oy + (float)item.Position.Y * scale,
                        Math.Max(2f, (float)item.Width * scale),
                        Math.Max(2f, (float)item.Height * scale));
                    g.FillRectangle(item.Solid ? solid : empty, rect);
                    g.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
                }

                DrawActor(g, ActorA, actorBrush, actorPen, headingPen, actorFont, ox, oy, scale);
                DrawActor(g, ActorB, actorBrush2, actorPen, headingPen, actorFont, ox, oy, scale);
                g.DrawString("SHARED AUTHORITATIVE WORLD", titleFont, Brushes.White, 4f, 5f);
                g.DrawString("A = sensor A   B = sensor B   •   actors move through the same collision rules", Font, Brushes.Gainsboro, 4f, ClientSize.Height - 20f);
            }
        }

        private static void DrawActor(Graphics g, WorldActor actor, Brush brush, Pen pen, Pen headingPen, Font font, float ox, float oy, float scale)
        {
            if (actor == null) return;
            var cx = ox + (float)actor.Position.X * scale;
            var cy = oy + (float)actor.Position.Y * scale;
            var radius = Math.Max(5f, (float)Math.Min(actor.Width, actor.Height) * scale * 0.55f);
            g.FillEllipse(brush, cx - radius, cy - radius, radius * 2f, radius * 2f);
            g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2f, radius * 2f);
            var angle = actor.RotationDegrees * Math.PI / 180.0;
            g.DrawLine(headingPen, cx, cy, cx + (float)Math.Cos(angle) * (radius + 8f), cy + (float)Math.Sin(angle) * (radius + 8f));
            g.DrawString(actor.Name, font, Brushes.White, cx + radius + 3f, cy - radius);
        }
    }
}
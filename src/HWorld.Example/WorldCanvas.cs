using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Example
{
    internal enum CanvasMode
    {
        Observe,
        Build,
        Play
    }

    internal sealed class WorldCanvas : Control
    {
        private World _world;
        private WorldActor _player;
        private float _zoom = 1f;
        private PointF _pan;
        private Point _lastMouse;
        private bool _panning;

        public WorldCanvas()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(10, 13, 17);
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public event EventHandler WorldEdited;

        public World World
        {
            get => _world;
            set { _world = value; FitWorld(); Invalidate(); }
        }

        public WorldActor Player
        {
            get => _player;
            set { _player = value; Invalidate(); }
        }

        public CanvasMode Mode { get; set; }
        public string BuildKind { get; set; } = "object";
        public bool BuildSolid { get; set; }
        public double BuildWidth { get; set; } = 8;
        public double BuildHeight { get; set; } = 8;
        public float Zoom => _zoom;

        public void ResetView() { FitWorld(); Invalidate(); }

        public void CenterOnPlayer()
        {
            if (_world == null || _player == null) return;
            var scale = GetScale();
            _pan = new PointF(ClientSize.Width / 2f - (float)_player.Position.X * scale - 24f,
                              ClientSize.Height / 2f - (float)_player.Position.Y * scale - 24f);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.Clear(BackColor);
            if (_world == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            var scale = GetScale();
            var origin = new PointF(24f + _pan.X, 24f + _pan.Y);
            var worldRect = new RectangleF(origin.X, origin.Y, (float)_world.Width * scale, (float)_world.Height * scale);

            using (var fill = new SolidBrush(Color.FromArgb(18, 24, 30)))
            using (var outline = new Pen(Color.FromArgb(90, 106, 122), 1.4f))
            {
                e.Graphics.FillRectangle(fill, worldRect);
                e.Graphics.DrawRectangle(outline, worldRect.X, worldRect.Y, worldRect.Width, worldRect.Height);
            }

            DrawGrid(e.Graphics, scale, origin, worldRect);
            DrawItems(e.Graphics, scale, origin);
            DrawPlayer(e.Graphics, scale, origin);

            if (Mode == CanvasMode.Build)
            {
                using (var pen = new Pen(Color.FromArgb(190, 115, 230, 255), 1f) { DashStyle = DashStyle.Dash })
                    e.Graphics.DrawRectangle(pen, 12, 12, ClientSize.Width - 24, ClientSize.Height - 24);
            }
        }

        private void DrawItems(Graphics g, float scale, PointF origin)
        {
            for (int i = 0; i < _world.Items.Count; i++)
            {
                var item = _world.Items[i];
                var rect = new RectangleF(origin.X + (float)item.Position.X * scale,
                                          origin.Y + (float)item.Position.Y * scale,
                                          Math.Max(4f, (float)item.Width * scale),
                                          Math.Max(4f, (float)item.Height * scale));

                Color fill;
                Color border;
                if (item.Solid) { fill = Color.FromArgb(210, 175, 83, 96); border = Color.FromArgb(235, 255, 157, 170); }
                else if (item.Kind == "nature") { fill = Color.FromArgb(215, 63, 153, 105); border = Color.FromArgb(240, 127, 219, 155); }
                else if (item.Kind == "resource") { fill = Color.FromArgb(220, 95, 125, 201); border = Color.FromArgb(240, 163, 190, 245); }
                else if (item.Kind == "landmark") { fill = Color.FromArgb(220, 157, 108, 211); border = Color.FromArgb(242, 218, 166, 247); }
                else { fill = Color.FromArgb(215, 68, 137, 198); border = Color.FromArgb(235, 139, 203, 246); }

                using (var brush = new SolidBrush(fill))
                using (var pen = new Pen(border, 1f))
                {
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        private void DrawPlayer(Graphics g, float scale, PointF origin)
        {
            if (_player == null) return;
            var cx = origin.X + (float)_player.Position.X * scale;
            var cy = origin.Y + (float)_player.Position.Y * scale;
            var radius = Math.Max(7f, (float)Math.Min(_player.Width, _player.Height) * scale * 0.6f);

            using (var glow = new SolidBrush(Color.FromArgb(35, 102, 224, 255)))
                g.FillEllipse(glow, cx - radius - 5, cy - radius - 5, (radius + 5) * 2, (radius + 5) * 2);
            using (var brush = new SolidBrush(Color.FromArgb(245, 93, 196, 255)))
            using (var pen = new Pen(Color.FromArgb(255, 222, 248, 255), 1.6f))
            {
                g.FillEllipse(brush, cx - radius, cy - radius, radius * 2, radius * 2);
                g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            var angle = (float)(_player.RotationDegrees * Math.PI / 180.0);
            var length = radius + 10;
            using (var pen = new Pen(Color.White, 2f))
                g.DrawLine(pen, cx, cy, cx + (float)Math.Cos(angle) * length, cy + (float)Math.Sin(angle) * length);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_world == null) return;
            var before = ScreenToWorld(e.Location);
            _zoom = Clamp(_zoom * (e.Delta > 0 ? 1.12f : 1f / 1.12f), 0.2f, 12f);
            var after = ScreenToWorld(e.Location);
            _pan.X += (after.X - before.X) * GetScale();
            _pan.Y += (after.Y - before.Y) * GetScale();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (Mode == CanvasMode.Build && e.Button == MouseButtons.Left)
            {
                var p = ScreenToWorld(e.Location);
                if (p.X >= 0 && p.Y >= 0 && p.X < _world.Width && p.Y < _world.Height)
                {
                    var item = _world.AddItem(new WorldPoint(p.X, p.Y), BuildWidth, BuildHeight, BuildSolid);
                    item.Kind = BuildKind;
                    item.Name = BuildKind;
                    WorldEdited?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                return;
            }

            if (Mode == CanvasMode.Build && e.Button == MouseButtons.Right)
            {
                var item = _world.FindItemAt(new WorldPoint(ScreenToWorld(e.Location).X, ScreenToWorld(e.Location).Y));
                if (item != null)
                {
                    _world.RemoveItem(item.Id);
                    WorldEdited?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                return;
            }

            if ((e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right) && Mode != CanvasMode.Build)
            {
                _panning = true;
                _lastMouse = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_panning) return;
            _pan.X += e.X - _lastMouse.X;
            _pan.Y += e.Y - _lastMouse.Y;
            _lastMouse = e.Location;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }
        }

        private float GetScale() => GetBaseScale() * _zoom;

        private float GetBaseScale()
        {
            if (_world == null || _world.Width <= 0 || _world.Height <= 0) return 1f;
            return Math.Max(0.05f, Math.Min((ClientSize.Width - 48f) / (float)_world.Width,
                                            (ClientSize.Height - 48f) / (float)_world.Height));
        }

        private void FitWorld()
        {
            if (_world == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            _zoom = 1f;
            var scale = GetBaseScale();
            var width = (float)_world.Width * scale;
            var height = (float)_world.Height * scale;
            _pan = new PointF((ClientSize.Width - 48f - width) / 2f,
                              (ClientSize.Height - 48f - height) / 2f);
        }

        private void DrawGrid(Graphics g, float scale, PointF origin, RectangleF worldRect)
        {
            var step = Math.Max(1, (int)Math.Ceiling(20f / Math.Max(scale, 0.01f)));
            var major = step * 5;
            using (var grid = new Pen(Color.FromArgb(17, 255, 255, 255)))
            using (var majorPen = new Pen(Color.FromArgb(34, 255, 255, 255)))
            {
                for (int x = 0; x <= _world.Width; x += step)
                {
                    var px = origin.X + x * scale;
                    if (px >= worldRect.Left && px <= worldRect.Right)
                        g.DrawLine(x % major == 0 ? majorPen : grid, px, worldRect.Top, px, worldRect.Bottom);
                }
                for (int y = 0; y <= _world.Height; y += step)
                {
                    var py = origin.Y + y * scale;
                    if (py >= worldRect.Top && py <= worldRect.Bottom)
                        g.DrawLine(y % major == 0 ? majorPen : grid, worldRect.Left, py, worldRect.Right, py);
                }
            }
        }

        private PointF ScreenToWorld(Point point)
        {
            var scale = GetScale();
            var origin = new PointF(24f + _pan.X, 24f + _pan.Y);
            return new PointF((point.X - origin.X) / scale, (point.Y - origin.Y) / scale);
        }

        private static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));
    }
}

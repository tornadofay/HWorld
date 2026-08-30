using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HWorld.Core.World;

namespace HWorld.Example
{
    internal sealed class WorldCanvas : Control
    {
        private World _world;
        private float _zoom = 1f;
        private PointF _pan;
        private Point _lastMouse;
        private bool _panning;

        public WorldCanvas()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(14, 17, 22);
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public World World
        {
            get { return _world; }
            set
            {
                _world = value;
                FitWorld();
                Invalidate();
            }
        }

        public float Zoom { get { return _zoom; } }

        public void ResetView()
        {
            FitWorld();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.Clear(BackColor);

            if (_world == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            var scale = GetScale();
            var origin = new PointF(24f + _pan.X, 24f + _pan.Y);
            var worldRect = new RectangleF(origin.X, origin.Y, (float)_world.Width * scale, (float)_world.Height * scale);

            using (var gridPen = new Pen(Color.FromArgb(24, 255, 255, 255), 1f))
            using (var majorPen = new Pen(Color.FromArgb(48, 255, 255, 255), 1f))
            {
                DrawGrid(e.Graphics, scale, origin, worldRect, gridPen, majorPen);
            }

            using (var fill = new SolidBrush(Color.FromArgb(17, 21, 27)))
            using (var outline = new Pen(Color.FromArgb(90, 105, 120), 1.5f))
            {
                e.Graphics.FillRectangle(fill, worldRect);
                e.Graphics.DrawRectangle(outline, worldRect.X, worldRect.Y, worldRect.Width, worldRect.Height);
            }

            foreach (var item in _world.Items)
            {
                var x = origin.X + (float)item.Position.X * scale;
                var y = origin.Y + (float)item.Position.Y * scale;
                var w = Math.Max(5f, (float)item.Width * scale);
                var h = Math.Max(5f, (float)item.Height * scale);
                var rect = new RectangleF(x, y, w, h);

                using (var fill = new SolidBrush(item.Solid ? Color.FromArgb(205, 206, 84, 96) : Color.FromArgb(215, 72, 151, 224)))
                using (var outline = new Pen(item.Solid ? Color.FromArgb(240, 255, 155, 165) : Color.FromArgb(240, 150, 210, 255), 1f))
                {
                    e.Graphics.FillRectangle(fill, rect);
                    e.Graphics.DrawRectangle(outline, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_world == null)
                return;

            var worldPointBefore = ScreenToWorld(e.Location);
            var factor = e.Delta > 0 ? 1.12f : 1f / 1.12f;
            _zoom = Clamp(_zoom * factor, 0.25f, 8f);
            var worldPointAfter = ScreenToWorld(e.Location);

            _pan.X += (worldPointAfter.X - worldPointBefore.X) * GetScale();
            _pan.Y += (worldPointAfter.Y - worldPointBefore.Y) * GetScale();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Middle && e.Button != MouseButtons.Right)
                return;

            Focus();
            _panning = true;
            _lastMouse = e.Location;
            Cursor = Cursors.SizeAll;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_panning)
                return;

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

        private float GetScale()
        {
            return GetBaseScale() * _zoom;
        }

        private float GetBaseScale()
        {
            if (_world == null || _world.Width <= 0 || _world.Height <= 0)
                return 1f;

            var sx = (ClientSize.Width - 48f) / (float)_world.Width;
            var sy = (ClientSize.Height - 48f) / (float)_world.Height;
            return Math.Max(0.1f, Math.Min(sx, sy));
        }

        private void FitWorld()
        {
            if (_world == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            _zoom = 1f;
            var scale = GetBaseScale();
            var width = (float)_world.Width * scale;
            var height = (float)_world.Height * scale;
            _pan = new PointF(
                (ClientSize.Width - 48f - width) / 2f,
                (ClientSize.Height - 48f - height) / 2f);
        }

        private void DrawGrid(Graphics g, float scale, PointF origin, RectangleF worldRect, Pen gridPen, Pen majorPen)
        {
            var stepWorld = Math.Max(1, (int)Math.Ceiling(24f / Math.Max(scale, 0.01f)));
            var majorStepWorld = stepWorld * 5;

            for (int x = 0; x <= _world.Width; x += stepWorld)
            {
                var px = origin.X + x * scale;
                if (px < 0 || px > ClientSize.Width) continue;
                g.DrawLine(x % majorStepWorld == 0 ? majorPen : gridPen, px, worldRect.Top, px, worldRect.Bottom);
            }

            for (int y = 0; y <= _world.Height; y += stepWorld)
            {
                var py = origin.Y + y * scale;
                if (py < 0 || py > ClientSize.Height) continue;
                g.DrawLine(y % majorStepWorld == 0 ? majorPen : gridPen, worldRect.Left, py, worldRect.Right, py);
            }
        }

        private PointF ScreenToWorld(Point point)
        {
            var scale = GetScale();
            var origin = new PointF(24f + _pan.X, 24f + _pan.Y);
            return new PointF(
                (point.X - origin.X) / scale,
                (point.Y - origin.Y) / scale);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}

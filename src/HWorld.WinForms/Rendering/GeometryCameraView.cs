using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.WinForms.Rendering
{
    /// <summary>
    /// Human-facing visualization of the renderer-independent geometry camera.
    /// The view displays only what the geometry sensor reports.
    /// </summary>
    public sealed class GeometryCameraView : Control
    {
        private static readonly SolidBrush BackgroundBrush = new SolidBrush(Color.FromArgb(10, 13, 17));
        private static readonly SolidBrush PlayerBrush = new SolidBrush(Color.FromArgb(245, 93, 196, 255));
        private static readonly Pen PlayerPen = new Pen(Color.White, 1.5f);
        private static readonly Pen FovPen = new Pen(Color.FromArgb(160, 104, 213, 255), 1.5f) { DashStyle = DashStyle.Dash };
        private static readonly SolidBrush FovBrush = new SolidBrush(Color.FromArgb(20, 104, 213, 255));
        private static readonly Pen RangePen = new Pen(Color.FromArgb(70, 104, 213, 255), 1f) { DashStyle = DashStyle.Dot };
        private static readonly Pen ObjectPen = new Pen(Color.FromArgb(210, 220, 230), 1.2f);
        private static readonly SolidBrush ObjectBrush = new SolidBrush(Color.FromArgb(190, 70, 130, 185));
        private static readonly SolidBrush SolidObjectBrush = new SolidBrush(Color.FromArgb(210, 166, 79, 92));
        private static readonly Pen TextPen = new Pen(Color.White, 1f);

        private readonly WorldGeometryCamera _camera = new WorldGeometryCamera();
        private readonly List<WorldGeometryObservation> _observations = new List<WorldGeometryObservation>(32);
        private World _world;
        private WorldActor _observer;

        public GeometryCameraView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = BackgroundBrush.Color;
            Camera = _camera;
        }

        public WorldGeometryCamera Camera { get; }
        public World World { get { return _world; } set { _world = value; Invalidate(); } }
        public WorldActor Observer { get { return _observer; } set { _observer = value; Invalidate(); } }
        public IReadOnlyList<WorldGeometryObservation> Observations { get { return _observations; } }

        public int RefreshObservation()
        {
            if (_world == null || _observer == null)
            {
                _observations.Clear();
                Invalidate();
                return 0;
            }

            var count = _camera.Observe(_world, _observer, _observations);
            Invalidate();
            return count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackgroundBrush.Color);

            if (_world == null || _observer == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            DrawSensor(g);
        }

        private void DrawSensor(Graphics g)
        {
            var scale = GetScale();
            var cx = ClientSize.Width * 0.5f;
            var cy = ClientSize.Height * 0.5f;

            var rangePx = (float)(Camera.Range * scale);
            var halfFov = Camera.FieldOfViewDegrees * 0.5;
            var heading = _observer.RotationDegrees * Math.PI / 180.0;

            using (var wedge = new GraphicsPath())
            {
                var left = heading - halfFov * Math.PI / 180.0;
                var pLeft = new PointF(cx + (float)Math.Cos(left) * rangePx, cy + (float)Math.Sin(left) * rangePx);
                wedge.AddLine(new PointF(cx, cy), pLeft);
                wedge.AddArc(cx - rangePx, cy - rangePx, rangePx * 2f, rangePx * 2f, (float)(left * 180.0 / Math.PI), (float)(Camera.FieldOfViewDegrees));
                wedge.CloseFigure();
                g.FillPath(FovBrush, wedge);
            }

            g.DrawEllipse(RangePen, cx - rangePx, cy - rangePx, rangePx * 2f, rangePx * 2f);
            g.DrawLine(FovPen, cx, cy,
                cx + (float)Math.Cos(heading - halfFov * Math.PI / 180.0) * rangePx,
                cy + (float)Math.Sin(heading - halfFov * Math.PI / 180.0) * rangePx);
            g.DrawLine(FovPen, cx, cy,
                cx + (float)Math.Cos(heading + halfFov * Math.PI / 180.0) * rangePx,
                cy + (float)Math.Sin(heading + halfFov * Math.PI / 180.0) * rangePx);

            for (int i = 0; i < _observations.Count; i++)
            {
                var observation = _observations[i];
                var x = cx + (float)(observation.RelativeX * scale);
                var y = cy + (float)(observation.RelativeY * scale);
                var w = Math.Max(4f, (float)(observation.Width * scale));
                var h = Math.Max(4f, (float)(observation.Height * scale));
                var fill = observation.Solid ? SolidObjectBrush : ObjectBrush;
                g.FillRectangle(fill, x - w * 0.5f, y - h * 0.5f, w, h);
                g.DrawRectangle(ObjectPen, x - w * 0.5f, y - h * 0.5f, w, h);
                g.DrawLine(TextPen, cx, cy, x, y);
            }

            const float playerRadius = 7f;
            g.FillEllipse(PlayerBrush, cx - playerRadius, cy - playerRadius, playerRadius * 2f, playerRadius * 2f);
            g.DrawEllipse(PlayerPen, cx - playerRadius, cy - playerRadius, playerRadius * 2f, playerRadius * 2f);
            var headingLength = playerRadius + 12f;
            g.DrawLine(PlayerPen, cx, cy, cx + (float)Math.Cos(heading) * headingLength, cy + (float)Math.Sin(heading) * headingLength);

            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                g.DrawString("GEOMETRY SENSOR", font, Brushes.White, 12f, 12f);
                g.DrawString(string.Format("FOV {0:0}°   Range {1:0.0}   Entities {2}", Camera.FieldOfViewDegrees, Camera.Range, _observations.Count), font, Brushes.Gainsboro, 12f, 32f);
            }
        }

        private float GetScale()
        {
            var maxWidth = Math.Max(1, ClientSize.Width - 48);
            var maxHeight = Math.Max(1, ClientSize.Height - 90);
            var sx = maxWidth / (float)Math.Max(1.0, Camera.Range * 2.0);
            var sy = maxHeight / (float)Math.Max(1.0, Camera.Range * 2.0);
            return Math.Max(0.5f, Math.Min(sx, sy));
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    // #1 fixed: paths cached in REAL coordinates. No Clone/Translate/Transform.
    internal sealed class GeometryCache : IDisposable
    {
        private GraphicsPath _fill, _stroke;
        private Rectangle _fillRect, _strokeRect;
        private int _fillRadius, _strokeRadius;
        private bool _fillRound, _strokeRound;

        public GraphicsPath GetFillPath(Rectangle rect, int radius, bool round)
            => GetPath(ref _fill, ref _fillRect, ref _fillRadius, ref _fillRound, rect, radius, round);

        public GraphicsPath GetStrokePath(Rectangle rect, int radius, bool round)
            => GetPath(ref _stroke, ref _strokeRect, ref _strokeRadius, ref _strokeRound, rect, radius, round);

        private static GraphicsPath GetPath(ref GraphicsPath path, ref Rectangle lastRect, ref int lastRadius,
            ref bool lastRound, Rectangle rect, int radius, bool round)
        {
            if (path == null || lastRect != rect || lastRadius != radius || lastRound != round)
            {
                path?.Dispose();
                path = round ? CreateRounded(rect, radius) : CreateRectangle(rect);
                lastRect = rect; lastRadius = radius; lastRound = round;
            }
            return path;
        }

        private static GraphicsPath CreateRounded(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0 || rect.Width <= 0 || rect.Height <= 0) { path.AddRectangle(rect); return path; }

            var arc = new Rectangle(rect.Location, new Size(d, d));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateRectangle(Rectangle rect)
        {
            var path = new GraphicsPath();
            path.AddRectangle(rect);
            return path;
        }

        public void Dispose() { _fill?.Dispose(); _stroke?.Dispose(); _fill = _stroke = null; }
    }
}

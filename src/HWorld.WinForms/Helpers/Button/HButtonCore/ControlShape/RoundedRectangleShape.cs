using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape
{
    internal static class RoundedRectangleShape
    {
        public static GraphicsPath CreatePath(Rectangle rect, int radius)
        {
            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            var path = new GraphicsPath();
            if (r <= 0 || rect.Width <= 0 || rect.Height <= 0) { path.AddRectangle(rect); return path; }

            int d = r * 2;
            var arc = new Rectangle(rect.Location, new Size(d, d));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

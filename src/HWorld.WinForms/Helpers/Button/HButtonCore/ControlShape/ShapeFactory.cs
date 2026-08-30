using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape
{
    internal static class ShapeFactory
    {
        public static GraphicsPath Create(Rectangle rect, int radius, bool round)
            => round ? RoundedRectangleShape.CreatePath(rect, radius) : RectangleShape.CreatePath(rect);
    }
}

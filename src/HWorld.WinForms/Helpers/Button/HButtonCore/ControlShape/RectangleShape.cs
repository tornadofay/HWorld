using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape
{
    internal static class RectangleShape
    {
        public static GraphicsPath CreatePath(Rectangle rect)
        {
            var p = new GraphicsPath();
            p.AddRectangle(rect);
            return p;
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape
{
    // #12: shape abstraction — ellipse/capsule/hexagon plug in later.
    public interface IShape { GraphicsPath CreatePath(Rectangle rect); }
}

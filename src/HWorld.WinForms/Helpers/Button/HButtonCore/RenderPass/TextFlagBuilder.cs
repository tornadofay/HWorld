using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    internal static class TextFlagBuilder
    {
        public static TextFormatFlags Build(in HButtonGeometry geo)
        {
            var f = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.WordEllipsis;
            if (geo.RightToLeft == RightToLeft.Yes) f |= TextFormatFlags.RightToLeft;
            switch (geo.TextAlign)
            {
                case ContentAlignment.TopLeft: f |= TextFormatFlags.Top | TextFormatFlags.Left; break;
                case ContentAlignment.TopCenter: f |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter; break;
                case ContentAlignment.TopRight: f |= TextFormatFlags.Top | TextFormatFlags.Right; break;
                case ContentAlignment.MiddleLeft: f |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left; break;
                case ContentAlignment.MiddleCenter: f |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter; break;
                case ContentAlignment.MiddleRight: f |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right; break;
                case ContentAlignment.BottomLeft: f |= TextFormatFlags.Bottom | TextFormatFlags.Left; break;
                case ContentAlignment.BottomCenter: f |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter; break;
                case ContentAlignment.BottomRight: f |= TextFormatFlags.Bottom | TextFormatFlags.Right; break;
            }
            return f;
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.VisualGraphics
{
    internal static class GraphicsConfigurator
    {
        public static void Apply(Graphics g, GraphicsProfile profile)
        {
            switch (profile)
            {
                case GraphicsProfile.Fast:
                    g.SmoothingMode = SmoothingMode.None;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.TextRenderingHint = TextRenderingHint.SystemDefault;
                    break;
                case GraphicsProfile.Printing:
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    break;
                default:
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    break;
            }
        }
    }
}

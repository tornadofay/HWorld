using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.VisualGraphics
{
    public enum GraphicsProfile { Default, HighQuality, Fast, Printing }

    public static class GraphicsProfileExtensions
    {
        public static void Apply(this GraphicsProfile profile, Graphics g)
        {
            switch (profile)
            {
                case GraphicsProfile.Fast: g.SmoothingMode = SmoothingMode.None; g.CompositingQuality = CompositingQuality.HighSpeed; g.InterpolationMode = InterpolationMode.NearestNeighbor; g.TextRenderingHint = TextRenderingHint.SystemDefault; break;
                case GraphicsProfile.Printing: g.SmoothingMode = SmoothingMode.AntiAlias; g.CompositingQuality = CompositingQuality.HighQuality; g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; break;
                default: g.SmoothingMode = SmoothingMode.AntiAlias; g.CompositingQuality = CompositingQuality.HighQuality; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit; break;
            }
        }
    }
}

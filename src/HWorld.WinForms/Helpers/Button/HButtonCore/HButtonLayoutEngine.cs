using HWorld.ImageCore;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button.HButtonCore
{
    internal sealed class HButtonLayoutEngine
    {
        public HButtonLayoutResult Layout(in HButtonGeometry geo, in HButtonContent content)
        {
            var bounds = geo.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return new HButtonLayoutResult(Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, false, false);

            float s = geo.ScaleFactor;
            int imgMargin = (int)(geo.ImageMargin * s);
            int txtMargin = (int)(geo.TextMargin * s);

            var bgRect = bounds;
            var borderRect = Rectangle.Inflate(bounds, -1, -1);

            bool hasImage = content.HasImage;
            bool hasText = content.HasText;

            // --- NEW: Handle ImageSizeMode ---
            var imgRect = Rectangle.Empty;
            if (hasImage)
            {
                switch (geo.ImageSizeMode)
                {
                    case ImageSizeMode.Stretch:
                        imgRect = Rectangle.Inflate(bounds, -imgMargin, -imgMargin);
                        break;

                    case ImageSizeMode.Zoom:
                        imgRect = ComputeZoomRect(bounds, content.Image, imgMargin);
                        break;

                    default: // Normal
                        int imgW = (int)(geo.ImageWidth * s);
                        int imgH = (int)(geo.ImageHeight * s);
                        imgRect = ComputeImageRect(bounds, imgW, imgH, geo.ImageAlign, geo.AlignmentMode, geo.RightToLeft, imgMargin);
                        break;
                }
            }

            var txtRect = hasText
                ? ComputeTextRect(bounds, imgRect, hasImage, geo.AlignmentMode, geo.RightToLeft, imgMargin, txtMargin)
                : Rectangle.Empty;

            return new HButtonLayoutResult(bgRect, borderRect, imgRect, txtRect, hasImage, hasText);
        }

        // NEW: Zoom helper
        private static Rectangle ComputeZoomRect(Rectangle bounds, Image image, int margin)
        {
            if (image == null) return Rectangle.Empty;

            int availW = bounds.Width - margin * 2;
            int availH = bounds.Height - margin * 2;
            if (availW <= 0 || availH <= 0) return Rectangle.Empty;

            float scale = Math.Min((float)availW / image.Width, (float)availH / image.Height);
            int w = (int)(image.Width * scale);
            int h = (int)(image.Height * scale);
            int x = bounds.X + (bounds.Width - w) / 2;
            int y = bounds.Y + (bounds.Height - h) / 2;

            return new Rectangle(x, y, w, h);
        }

        private static Rectangle ComputeImageRect(Rectangle b, int w, int h, ContentAlignment align, AlignmentType mode, RightToLeft rtl, int margin)
        {
            if (mode == AlignmentType.AutoAlignment) { int x = rtl == RightToLeft.Yes ? b.Right - w - margin : b.Left + margin; int y = b.Top + (b.Height - h) / 2; return new Rectangle(x, y, w, h); }
            int px, py;
            switch (align)
            {
                case ContentAlignment.TopLeft: px = b.Left + margin; py = b.Top + margin; break;
                case ContentAlignment.TopCenter: px = b.Left + (b.Width - w) / 2; py = b.Top + margin; break;
                case ContentAlignment.TopRight: px = b.Right - w - margin; py = b.Top + margin; break;
                case ContentAlignment.MiddleLeft: px = b.Left + margin; py = b.Top + (b.Height - h) / 2; break;
                case ContentAlignment.MiddleCenter: px = b.Left + (b.Width - w) / 2; py = b.Top + (b.Height - h) / 2; break;
                case ContentAlignment.MiddleRight: px = b.Right - w - margin; py = b.Top + (b.Height - h) / 2; break;
                case ContentAlignment.BottomLeft: px = b.Left + margin; py = b.Bottom - h - margin; break;
                case ContentAlignment.BottomCenter: px = b.Left + (b.Width - w) / 2; py = b.Bottom - h - margin; break;
                default: px = b.Right - w - margin; py = b.Bottom - h - margin; break;
            }
            return new Rectangle(px, py, w, h);
        }

        private static Rectangle ComputeTextRect(Rectangle b, Rectangle img, bool hasImage, AlignmentType mode, RightToLeft rtl, int imgMargin, int txtMargin)
        {
            if (mode == AlignmentType.AutoAlignment)
            {
                int x = b.Left + txtMargin; int width = b.Width - txtMargin * 2;
                if (hasImage) { if (rtl == RightToLeft.Yes) { width = img.Left - txtMargin - b.Left; x = b.Left + txtMargin; } else { x = img.Right + imgMargin; width = b.Right - txtMargin - x; } }
                if (width < 0) width = 0;
                return new Rectangle(x, b.Top + txtMargin, width, b.Height - txtMargin * 2);
            }
            return Rectangle.Inflate(b, -txtMargin, -txtMargin);
        }
    }
}

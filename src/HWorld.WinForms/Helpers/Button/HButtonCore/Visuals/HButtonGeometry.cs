using HWorld.ImageCore;
using System.Drawing;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonGeometry
    {
        public readonly Rectangle Bounds;
        public readonly int ImageWidth, ImageHeight, ImageMargin, TextMargin;
        public readonly ContentAlignment ImageAlign, TextAlign;
        public readonly AlignmentType AlignmentMode;
        public readonly RightToLeft RightToLeft;
        public readonly float ScaleFactor;
        public readonly ImageSizeMode ImageSizeMode;

        public HButtonGeometry(Rectangle bounds, int imgW, int imgH, int imgMargin, int txtMargin,
        ContentAlignment imgAlign, ContentAlignment txtAlign, AlignmentType alignMode,
        RightToLeft rtl, float scale, ImageSizeMode sizeMode)
        {
            Bounds = bounds; ImageWidth = imgW; ImageHeight = imgH;
            ImageMargin = imgMargin; TextMargin = txtMargin;
            ImageAlign = imgAlign; TextAlign = txtAlign;
            AlignmentMode = alignMode; RightToLeft = rtl;
            ScaleFactor = scale; ImageSizeMode = sizeMode;
        }
    }
}

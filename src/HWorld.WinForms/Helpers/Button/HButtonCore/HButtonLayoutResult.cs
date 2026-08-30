using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore
{
    public readonly struct HButtonLayoutResult
    {
        public readonly Rectangle BackgroundRect, BorderRect, ImageRect, TextRect;
        public readonly bool HasImage, HasText;
        public HButtonLayoutResult(Rectangle bg, Rectangle border, Rectangle image, Rectangle text, bool hasImage, bool hasText)
        { BackgroundRect = bg; BorderRect = border; ImageRect = image; TextRect = text; HasImage = hasImage; HasText = hasText; }
    }
}

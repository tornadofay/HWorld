using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonContent
    {
        public readonly Image Image;
        public readonly string Text;
        public readonly Font Font;
        public HButtonContent(Image image, string text, Font font) { Image = image; Text = text; Font = font; }
        public bool HasImage => Image != null;
        public bool HasText => !string.IsNullOrEmpty(Text);
    }
}

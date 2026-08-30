using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    // Immutable snapshot of resolved colors. Renderer consumes it; no if/else trees.
    public readonly struct ButtonVisual
    {
        public readonly Color Background1;
        public readonly Color Background2;
        public readonly Color Foreground;
        public readonly Color Border;

        public ButtonVisual(Color bg1, Color bg2, Color fg, Color border)
        { Background1 = bg1; Background2 = bg2; Foreground = fg; Border = border; }
    }
}

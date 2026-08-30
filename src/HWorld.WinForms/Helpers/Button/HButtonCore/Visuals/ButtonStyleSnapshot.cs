using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct ButtonStyleSnapshot
    {
        public readonly Color Background1;
        public readonly Color Background2;
        public readonly Color Foreground;
        public readonly Color Border;
        public readonly float GradientAngle;

        public ButtonStyleSnapshot(Color bg1, Color bg2, Color fg, Color border, float angle)
        { Background1 = bg1; Background2 = bg2; Foreground = fg; Border = border; GradientAngle = angle; }
    }
}

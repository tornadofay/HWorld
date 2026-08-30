using System;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    [Flags]
    public enum ButtonVisualState
    {
        Normal = 0, Hover = 1, Pressed = 2, Disabled = 4,
        Focused = 8, Default = 16, Checked = 32, HotTracked = 64
    }
}

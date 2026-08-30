using System;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    // Internal: Not exposed on the public IHButtonRenderer interface.
    [System.Flags]
    public enum RendererCapabilities
    {
        None = 0, RoundedCorners = 1, Gradients = 2, Shadows = 4,
        Animations = 8, Ripple = 16, HighDpi = 32, Transparency = 64
    }
}

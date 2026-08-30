using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Themes
{
    internal static class HButtonStyleResolver
    {
        public static ButtonStyleSnapshot Resolve(in HButtonRenderContext ctx)
        {
            var state = ctx.State.VisualState;

            // Disabled State
            if ((state & ButtonVisualState.Disabled) != 0)
            {
                return new ButtonStyleSnapshot(SystemColors.Control, SystemColors.ControlDark, SystemColors.GrayText, SystemColors.ControlDark, 90f);
            }

            // Custom Colors Override
            var c = ctx.Appearance.CustomColors;
            if ((state & ButtonVisualState.Pressed) != 0) return new ButtonStyleSnapshot(c.DownBg1, c.DownBg2, c.DownFore, c.DownBorder, 90f);
            if ((state & ButtonVisualState.Hover) != 0) return new ButtonStyleSnapshot(c.EnterBg1, c.EnterBg2, c.EnterFore, c.EnterBorder, 90f);
            return new ButtonStyleSnapshot(c.LeaveBg1, c.LeaveBg2, c.LeaveFore, c.LeaveBorder, 90f);

        }
    }
}

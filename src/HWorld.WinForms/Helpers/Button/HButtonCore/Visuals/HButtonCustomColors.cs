using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonCustomColors
    {
        public readonly Color LeaveBg1, LeaveBg2, LeaveFore, LeaveBorder;
        public readonly Color EnterBg1, EnterBg2, EnterFore, EnterBorder;
        public readonly Color DownBg1, DownBg2, DownFore, DownBorder;

        public HButtonCustomColors(Color lb1, Color lb2, Color lf, Color lbo, Color eb1, Color eb2, Color ef, Color ebo, Color db1, Color db2, Color df, Color dbo)
        { LeaveBg1 = lb1; LeaveBg2 = lb2; LeaveFore = lf; LeaveBorder = lbo; EnterBg1 = eb1; EnterBg2 = eb2; EnterFore = ef; EnterBorder = ebo; DownBg1 = db1; DownBg2 = db2; DownFore = df; DownBorder = dbo; }

        public ButtonStyleSnapshot GetStyle(ButtonVisualState state)
        {
            if ((state & ButtonVisualState.Disabled) != 0) return new ButtonStyleSnapshot(SystemColors.Control, SystemColors.ControlDark, SystemColors.GrayText, SystemColors.ControlDark, 90f);
            if ((state & ButtonVisualState.Pressed) != 0) return new ButtonStyleSnapshot(DownBg1, DownBg2, DownFore, DownBorder, 90f);
            if ((state & ButtonVisualState.Hover) != 0) return new ButtonStyleSnapshot(EnterBg1, EnterBg2, EnterFore, EnterBorder, 90f);
            return new ButtonStyleSnapshot(LeaveBg1, LeaveBg2, LeaveFore, LeaveBorder, 90f);
        }
    }
}

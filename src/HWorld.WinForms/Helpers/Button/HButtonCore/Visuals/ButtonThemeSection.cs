namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{

    public sealed class ButtonThemeSection
    {
        public ButtonStyleSnapshot Leave { get; }
        public ButtonStyleSnapshot Enter { get; }
        public ButtonStyleSnapshot Down { get; }
        public ButtonStyleSnapshot Disabled { get; }

        public ButtonThemeSection(ButtonStyleSnapshot leave, ButtonStyleSnapshot enter,
                                  ButtonStyleSnapshot down, ButtonStyleSnapshot disabled)
        { Leave = leave; Enter = enter; Down = down; Disabled = disabled; }

        public ButtonStyleSnapshot GetStyle(ButtonVisualState state)
        {
            if ((state & ButtonVisualState.Disabled) != 0) return Disabled;
            if ((state & ButtonVisualState.Pressed) != 0) return Down;
            if ((state & ButtonVisualState.Hover) != 0) return Enter;
            return Leave;
        }
    }

}

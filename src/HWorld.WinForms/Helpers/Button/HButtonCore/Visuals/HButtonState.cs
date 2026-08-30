namespace HWorld.WinForms.Helpers.Button.HButtonCore.Visuals
{
    public readonly struct HButtonState
    {
        public readonly ButtonVisualState VisualState;
        public readonly bool ShowFocusCues;
        public HButtonState(ButtonVisualState s, bool focusCues) { VisualState = s; ShowFocusCues = focusCues; }
        public bool IsDisabled => (VisualState & ButtonVisualState.Disabled) != 0;
        public bool IsPressed => (VisualState & ButtonVisualState.Pressed) != 0;
        public bool IsHovered => (VisualState & ButtonVisualState.Hover) != 0;
        public bool IsFocused => (VisualState & ButtonVisualState.Focused) != 0;
    }
}

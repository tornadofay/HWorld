using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System;


namespace HWorld.WinForms.Helpers.Button.HButtonCore
{
    internal sealed class HButtonStateMachine
    {
        private ButtonVisualState _state = ButtonVisualState.Normal;
        public ButtonVisualState State => _state;
        public event EventHandler StateChanged;
        private void Update(ButtonVisualState next)
        {
            if ((next & ButtonVisualState.Disabled) != 0) next &= ~(ButtonVisualState.Hover | ButtonVisualState.Pressed);
            if (next != _state) { _state = next; StateChanged?.Invoke(this, EventArgs.Empty); }
        }
        public void MouseEnter() { if ((_state & ButtonVisualState.Disabled) != 0) return; Update(_state | ButtonVisualState.Hover); }
        public void MouseLeave() { if ((_state & ButtonVisualState.Pressed) != 0) return; Update(_state & ~ButtonVisualState.Hover); }
        public void MouseDown() { if ((_state & ButtonVisualState.Disabled) != 0) return; Update(_state | ButtonVisualState.Pressed); }
        public void MouseUp(bool isOver) { var next = _state & ~ButtonVisualState.Pressed; if (isOver && (_state & ButtonVisualState.Disabled) == 0) next |= ButtonVisualState.Hover; else next &= ~ButtonVisualState.Hover; Update(next); }
        public void KeyDownSpace() { if ((_state & ButtonVisualState.Disabled) != 0) return; Update(_state | ButtonVisualState.Pressed); }
        public void KeyUpSpace() => Update(_state & ~ButtonVisualState.Pressed);
        public void FocusGained() => Update(_state | ButtonVisualState.Focused);
        public void FocusLost() => Update(_state & ~ButtonVisualState.Focused);
        public void SetEnabled(bool enabled) { if (enabled) Update(_state & ~ButtonVisualState.Disabled); else Update(_state | ButtonVisualState.Disabled); }
    }
}

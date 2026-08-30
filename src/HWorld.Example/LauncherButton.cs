using System.Drawing;
using System.Windows.Forms;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    internal sealed class LauncherButton : HButton
    {
        public LauncherButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            ButtonLeaveForeColor = Color.White;
            ButtonEnterForeColor = Color.White;
            ButtonDownForeColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (string.IsNullOrWhiteSpace(Text)) return;

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                Enabled ? Color.White : SystemColors.GrayText,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.EndEllipsis);
        }
    }
}

using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    public sealed class FocusPass : IRenderPass
    {
        public RenderStage Stage => RenderStage.Focus;
        public void Execute(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout, in ButtonStyleSnapshot style)
        {
            if (!ctx.State.ShowFocusCues || !ctx.State.IsFocused || layout.TextRect.IsEmpty) return;
            var fr = Rectangle.Inflate(layout.TextRect, -2, -2); if (fr.Width > 0 && fr.Height > 0) ControlPaint.DrawFocusRectangle(g, fr);
        }
    }
}

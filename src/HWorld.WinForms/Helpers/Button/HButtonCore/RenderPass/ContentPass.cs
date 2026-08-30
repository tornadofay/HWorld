using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.VisualGraphics;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;
using System.Windows.Forms;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    public sealed class ContentPass : IRenderPass
    {
        public RenderStage Stage => RenderStage.Content;
        public void Execute(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout, in ButtonStyleSnapshot style)
        {
            if (layout.HasImage) { if (ctx.State.IsDisabled) ControlPaint.DrawImageDisabled(g, ctx.Content.Image, layout.ImageRect.X, layout.ImageRect.Y, Color.Transparent); else g.DrawImage(ctx.Content.Image, layout.ImageRect); }
            if (layout.HasText) { var flags = TextFlagBuilder.Build(in ctx.Geometry); if (ctx.State.IsDisabled) ControlPaint.DrawStringDisabled(g, ctx.Content.Text, ctx.Content.Font, style.Foreground, layout.TextRect, flags); else TextRenderer.DrawText(g, ctx.Content.Text, ctx.Content.Font, layout.TextRect, style.Foreground, flags); }
        }
    }
}

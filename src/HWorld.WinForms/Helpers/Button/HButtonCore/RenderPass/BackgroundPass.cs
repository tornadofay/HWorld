using HWorld.WinForms.Helpers.Button.HButtonCore.Cashes;
using HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape;
using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    public sealed class BackgroundPass : IRenderPass
    {
        private readonly GradientBrushCache _brushes;
        public BackgroundPass(GradientBrushCache brushes) => _brushes = brushes;
        public RenderStage Stage => RenderStage.Background;

        public void Execute(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout, in ButtonStyleSnapshot style)
        {
            var rect = layout.BackgroundRect;
            if (rect.IsEmpty) return;

            using (var path = ShapeFactory.Create(rect, ctx.Appearance.Radius, ctx.Appearance.RoundButton))
            {
                if (ctx.Appearance.RoundStyle)
                {
                    // RESTORED: Original Radial Gradient
                    using (var pgb = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        pgb.CenterPoint = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                        pgb.CenterColor = style.Background1;
                        pgb.SurroundColors = new[] { style.Background2 };
                        g.FillPath(pgb, path);
                    }
                }
                else
                {
                    // Standard Linear Gradient
                    var brush = _brushes.Get(rect.Size, style.Background1, style.Background2, style.GradientAngle);
                    if (brush != null) g.FillPath(brush, path);
                }
            }
        }
    }
}

using HWorld.WinForms.Helpers.Button.HButtonCore.Cashes;
using HWorld.WinForms.Helpers.Button.HButtonCore.ControlShape;  
using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    public sealed class BorderPass : IRenderPass
    {
        private readonly PenCache _pens;
        public BorderPass(PenCache pens) => _pens = pens;
        public RenderStage Stage => RenderStage.Border;

        public void Execute(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout, in ButtonStyleSnapshot style)
        {
            var rect = layout.BorderRect;
            if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0) return;

            using (var path = ShapeFactory.Create(rect, ctx.Appearance.Radius, ctx.Appearance.RoundButton))
            {
                // Draw the actual border
                var pen = _pens.Get(style.Border, 1f, DashStyle.Solid);
                g.DrawPath(pen, path);

                // QUALITY FIX: Draw a 1px semi-transparent overlay on the outer edge
                // This smooths the jagged Region clipping
                if (ctx.Appearance.RoundButton)
                {
                    var outerRect = Rectangle.Inflate(rect, 1, 1);
                    using (var outerPath = ShapeFactory.Create(outerRect, ctx.Appearance.Radius + 1, true))
                    using (var softPen = new Pen(Color.FromArgb(40, style.Border), 1.5f))
                    {
                        g.DrawPath(softPen, outerPath);
                    }
                }
            }
        }
    }
}

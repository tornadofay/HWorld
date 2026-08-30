using HWorld.WinForms.Helpers.Button.HButtonCore.Rendering;
using HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass;
using HWorld.WinForms.Helpers.Button.HButtonCore.VisualGraphics;
using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore
{
    public sealed class HButtonClassicRenderer : IHButtonRenderer
    {
        private readonly IRenderPass[] _passes;
        public HButtonClassicRenderer(ClassicRendererResources resources) { _passes = HButtonClassicRendererFactory.CreatePasses(resources); }
        public uint LayoutVersion => 1;
        public void Draw(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout)
        {
            var style = HButtonStyleResolver.Resolve(in ctx);
            var saved = g.Save();
            try { GraphicsProfile.HighQuality.Apply(g); g.SetClip(ctx.Geometry.Bounds); for (int i = 0; i < _passes.Length; i++) _passes[i].Execute(g, in ctx, in layout, in style); }
            finally { g.Restore(saved); }
        }
        public void Dispose() { }
    }
}

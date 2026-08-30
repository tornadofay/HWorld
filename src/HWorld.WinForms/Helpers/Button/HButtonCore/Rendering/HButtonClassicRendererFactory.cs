using HWorld.WinForms.Helpers.Button.HButtonCore.RenderPass;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    internal static class HButtonClassicRendererFactory
    {
        public static IRenderPass[] CreatePasses(ClassicRendererResources res)
        {
            return new IRenderPass[] { new BackgroundPass(res.Brushes), new BorderPass(res.Pens), new ContentPass(), new FocusPass() };
        }
    }
}

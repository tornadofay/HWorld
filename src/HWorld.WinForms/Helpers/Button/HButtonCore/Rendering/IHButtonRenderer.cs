using HWorld.WinForms.Helpers.Button.HButtonCore.Visuals;
using System;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    public interface IHButtonRenderer : IDisposable
    {
        uint LayoutVersion { get; }
        void Draw(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout);
    }
}

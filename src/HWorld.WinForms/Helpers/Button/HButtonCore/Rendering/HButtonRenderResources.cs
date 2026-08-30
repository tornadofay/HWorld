using HWorld.WinForms.Helpers.Button.HButtonCore.Cashes;
using System;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    public class HButtonRendererResources : IDisposable
    {
        public PenCache Pens { get; } = new PenCache();
        public GradientBrushCache Brushes { get; } = new GradientBrushCache();
        public virtual void Dispose() { Pens.Dispose(); Brushes.Dispose(); }
    }
}

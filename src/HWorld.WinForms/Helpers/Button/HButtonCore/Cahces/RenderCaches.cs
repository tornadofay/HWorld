using System;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    // #12 fixed: caches are separate responsibilities, bundled for ownership.
    internal sealed class RenderCaches : IDisposable
    {
        public GeometryCache Geometry { get; } = new GeometryCache();
        public BrushCache Brushes { get; } = new BrushCache();
        public PenCache Pens { get; } = new PenCache();

        public void Dispose() { Geometry.Dispose(); Brushes.Dispose(); Pens.Dispose(); }
    }
}

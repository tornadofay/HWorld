using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.VisualGraphics
{
    /// <summary>
    /// Saves and restores Graphics state for passes that need temporary overrides.
    /// 99% of passes don't need this; use only when a pass requires different graphics settings.
    /// </summary>
    internal readonly struct GraphicsStateScope : IDisposable
    {
        private readonly Graphics _g;
        private readonly GraphicsState _state;

        public GraphicsStateScope(Graphics g, GraphicsProfile profile)
        {
            _g = g;
            _state = g.Save();
            GraphicsConfigurator.Apply(g, profile);
        }

        public void Dispose() => _g.Restore(_state);
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    // #2 + #3 fixed: independent state, compares full Rectangle + colors.
    internal sealed class BrushCache : IDisposable
    {
        private LinearGradientBrush _brush;
        private Rectangle _lastRect;
        private Color _lastC1, _lastC2;

        public Brush GetGradient(Rectangle rect, Color c1, Color c2)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return null;
            if (_brush == null || _lastRect != rect || _lastC1 != c1 || _lastC2 != c2)
            {
                _brush?.Dispose();
                _brush = new LinearGradientBrush(rect, c1, c2, 90f);
                _lastRect = rect; _lastC1 = c1; _lastC2 = c2;
            }
            return _brush;
        }

        public void Dispose() { _brush?.Dispose(); _brush = null; }
    }
}

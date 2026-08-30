using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    // Size-based key: X/Y don't affect gradient shape in local coordinates.
    internal readonly struct GradientBrushKey : IEquatable<GradientBrushKey>
    {
        public readonly Size Size; public readonly Color C1, C2; public readonly float Angle;
        public GradientBrushKey(Size s, Color c1, Color c2, float a) { Size = s; C1 = c1; C2 = c2; Angle = a; }
        public bool Equals(GradientBrushKey o) => Size == o.Size && C1 == o.C1 && C2 == o.C2 && Angle == o.Angle;
        public override bool Equals(object o) => o is GradientBrushKey k && Equals(k);
        public override int GetHashCode() => Size.GetHashCode() ^ C1.GetHashCode() ^ C2.GetHashCode() ^ Angle.GetHashCode();
    }

    public sealed class GradientBrushCache : IDisposable
    {
        private readonly BoundedCache<GradientBrushKey, LinearGradientBrush> _brushes = new BoundedCache<GradientBrushKey, LinearGradientBrush>(64);

        public Brush Get(Size size, Color c1, Color c2, float angle)
        {
            if (size.Width <= 0 || size.Height <= 0) return null;
            var key = new GradientBrushKey(size, c1, c2, angle);
            if (_brushes.TryGet(key, out var brush)) return brush;
            brush = new LinearGradientBrush(new Rectangle(Point.Empty, size), c1, c2, angle);
            _brushes.Add(key, brush);
            return brush;
        }
        public void Dispose() => _brushes.Dispose();
    }
}

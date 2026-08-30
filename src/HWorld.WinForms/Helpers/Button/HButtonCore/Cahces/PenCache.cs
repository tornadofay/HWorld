using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    internal readonly struct PenKey : IEquatable<PenKey>
    {
        public readonly Color Color; public readonly float Width; public readonly DashStyle Dash;
        public PenKey(Color c, float w, DashStyle d) { Color = c; Width = w; Dash = d; }
        public bool Equals(PenKey o) => Color == o.Color && Width == o.Width && Dash == o.Dash;
        public override bool Equals(object o) => o is PenKey k && Equals(k);
        // public override int GetHashCode() => Color.GetHashCode() ^ Width.GetHashCode() ^ Dash.GetHashCode();
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Color.GetHashCode();
                hash = hash * 397 + Width.GetHashCode();
                hash = hash * 397 + (int)Dash;
                return hash;
            }
        }
    }

    public sealed class PenCache : IDisposable
    {
        private readonly BoundedCache<PenKey, Pen> _pens = new BoundedCache<PenKey, Pen>(128);
        public Pen Get(Color color, float width = 1f, DashStyle dash = DashStyle.Solid)
        {
            var key = new PenKey(color, width, dash);
            if (_pens.TryGet(key, out var pen)) return pen;
            pen = new Pen(color, width) { DashStyle = dash };
            _pens.Add(key, pen);
            return pen;
        }
        public void Dispose() => _pens.Dispose();
    }
}

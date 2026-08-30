using System;
using System.Drawing;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    internal readonly struct ShapePathKey : IEquatable<ShapePathKey>
    {
        public readonly Rectangle Rect; public readonly int Radius; public readonly bool Round;
        public ShapePathKey(Rectangle r, int radius, bool round) { Rect = r; Radius = radius; Round = round; }
        public bool Equals(ShapePathKey o) => Rect == o.Rect && Radius == o.Radius && Round == o.Round;
        public override bool Equals(object o) => o is ShapePathKey k && Equals(k);
        public override int GetHashCode() => Rect.GetHashCode() ^ Radius.GetHashCode() ^ Round.GetHashCode();
    }
}

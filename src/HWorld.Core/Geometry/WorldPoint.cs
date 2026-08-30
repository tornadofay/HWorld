namespace HWorld.Core.Geometry
{
    public readonly struct WorldPoint
    {
        public WorldPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static WorldPoint operator +(WorldPoint a, WorldPoint b)
            => new WorldPoint(a.X + b.X, a.Y + b.Y);

        public static WorldPoint operator -(WorldPoint a, WorldPoint b)
            => new WorldPoint(a.X - b.X, a.Y - b.Y);
    }
}

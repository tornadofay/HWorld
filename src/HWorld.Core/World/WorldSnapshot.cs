using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class WorldSnapshot
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double SimulationTime { get; set; }
        public List<WorldItemSnapshot> Items { get; set; } = new List<WorldItemSnapshot>();
        public List<WorldActorSnapshot> Actors { get; set; } = new List<WorldActorSnapshot>();
    }

    public sealed class WorldItemSnapshot
    {
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double RotationDegrees { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Solid { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public WorldShapeKind Shape { get; set; }
        public byte VisualVariant { get; set; }
    }

    public sealed class WorldActorSnapshot
    {
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double RotationDegrees { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Speed { get; set; }
        public bool Collides { get; set; }
        public string Name { get; set; }
    }
}

using System;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class WorldItem
    {
        public WorldItem(Guid id, WorldPoint position)
        {
            Id = id;
            Position = position;
        }

        public Guid Id { get; }
        public WorldPoint Position { get; set; }
        public double RotationDegrees { get; set; }
        public double Width { get; set; } = 1;
        public double Height { get; set; } = 1;
        public bool Solid { get; set; }

        // Simulation semantics are renderer-independent. The example maps these
        // values to visuals, while future renderers may represent them differently.
        public string Kind { get; set; } = "object";
        public string Name { get; set; } = "Object";
    }
}

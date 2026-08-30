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

        // Simulation semantics are renderer-independent.
        public string Kind { get; set; } = "object";
        public string Name { get; set; } = "Object";

        // Vector presentation hint. The world stores the shape; each renderer
        // decides how to paint it. Keeping this as an enum avoids graphics/UI
        // dependencies in HWorld.Core.
        public WorldShapeKind Shape { get; set; } = WorldShapeKind.Rectangle;

        // Small deterministic variation value for renderers that want subtle
        // visual differences without changing the object's simulation meaning.
        public byte VisualVariant { get; set; }
    }
}

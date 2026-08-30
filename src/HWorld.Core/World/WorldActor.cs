using System;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class WorldActor
    {
        public WorldActor(Guid id, WorldPoint position)
        {
            Id = id;
            Position = position;
        }

        public Guid Id { get; }
        public WorldPoint Position { get; internal set; }
        public double RotationDegrees { get; set; }
        public double Width { get; set; } = 1.0;
        public double Height { get; set; } = 1.0;
        public double Speed { get; set; } = 5.0;
        public bool Collides { get; set; } = true;
        public string Name { get; set; } = "Actor";
    }
}
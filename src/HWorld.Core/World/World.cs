using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class World
    {
        private readonly List<WorldItem> _items = new List<WorldItem>();

        public World(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; }
        public double Height { get; }
        public double SimulationTime { get; private set; }
        public IReadOnlyList<WorldItem> Items => _items;

        public WorldItem AddItem(WorldPoint position, double width = 1, double height = 1, bool solid = false)
        {
            var item = new WorldItem(Guid.NewGuid(), position)
            {
                Width = width,
                Height = height,
                Solid = solid
            };

            _items.Add(item);
            return item;
        }

        public void Update(double deltaSeconds)
        {
            if (deltaSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            SimulationTime += deltaSeconds;
        }
    }
}

using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class World
    {
        private readonly List<WorldItem> _items = new List<WorldItem>();
        private readonly List<WorldActor> _actors = new List<WorldActor>();

        public World(double width, double height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
        }

        public double Width { get; }
        public double Height { get; }
        public double SimulationTime { get; private set; }
        public IReadOnlyList<WorldItem> Items => _items;
        public IReadOnlyList<WorldActor> Actors => _actors;

        public WorldItem AddItem(WorldPoint position, double width = 1, double height = 1, bool solid = false)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            var item = new WorldItem(Guid.NewGuid(), position)
            {
                Width = width,
                Height = height,
                Solid = solid
            };

            _items.Add(item);
            return item;
        }

        internal WorldItem RestoreItem(WorldItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Width <= 0) throw new ArgumentOutOfRangeException(nameof(item));
            if (item.Height <= 0) throw new ArgumentOutOfRangeException(nameof(item));
            _items.Add(item);
            return item;
        }

        public bool RemoveItem(Guid id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public WorldActor AddActor(WorldPoint position, double width = 1.6, double height = 1.6, double speed = 5.0)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (speed < 0) throw new ArgumentOutOfRangeException(nameof(speed));

            var actor = new WorldActor(Guid.NewGuid(), position)
            {
                Width = width,
                Height = height,
                Speed = speed
            };

            if (!IsInsideWorld(actor, position))
                throw new ArgumentOutOfRangeException(nameof(position));

            if (actor.Collides && IntersectsSolidItem(actor, position))
                throw new InvalidOperationException("The actor cannot be spawned inside a solid world item.");

            _actors.Add(actor);
            return actor;
        }

        internal WorldActor RestoreActor(WorldActor actor)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (actor.Width <= 0) throw new ArgumentOutOfRangeException(nameof(actor));
            if (actor.Height <= 0) throw new ArgumentOutOfRangeException(nameof(actor));
            if (actor.Speed < 0) throw new ArgumentOutOfRangeException(nameof(actor));
            if (!IsInsideWorld(actor, actor.Position)) throw new ArgumentOutOfRangeException(nameof(actor));
            _actors.Add(actor);
            return actor;
        }

        public bool RemoveActor(Guid id)
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i].Id == id)
                {
                    _actors.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool MoveActor(Guid actorId, double deltaX, double deltaY, double deltaSeconds)
        {
            WorldActor actor = FindActor(actorId);
            if (actor == null) return false;
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length > 0)
            {
                var maxDistance = actor.Speed * deltaSeconds;
                if (length > maxDistance)
                {
                    var scale = maxDistance / length;
                    deltaX *= scale;
                    deltaY *= scale;
                }
            }

            var target = new WorldPoint(actor.Position.X + deltaX, actor.Position.Y + deltaY);
            var moved = false;

            // Resolve each axis separately so the actor can slide along walls.
            var xTarget = new WorldPoint(target.X, actor.Position.Y);
            if (CanOccupy(actor, xTarget))
            {
                actor.Position = xTarget;
                moved = Math.Abs(deltaX) > 0.000001;
            }

            var yTarget = new WorldPoint(actor.Position.X, target.Y);
            if (CanOccupy(actor, yTarget))
            {
                actor.Position = yTarget;
                moved = moved || Math.Abs(deltaY) > 0.000001;
            }

            if (moved)
                actor.RotationDegrees = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;

            return moved;
        }

        public WorldActor FindActor(Guid id)
        {
            for (int i = 0; i < _actors.Count; i++)
                if (_actors[i].Id == id)
                    return _actors[i];
            return null;
        }

        public WorldItem FindItemAt(WorldPoint point)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (Contains(item, point))
                    return item;
            }

            return null;
        }

        public void Update(double deltaSeconds)
        {
            if (deltaSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            SimulationTime += deltaSeconds;
        }

        internal void SetSimulationTime(double value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            SimulationTime = value;
        }

        private bool CanOccupy(WorldActor actor, WorldPoint center)
        {
            if (!IsInsideWorld(actor, center))
                return false;

            return !actor.Collides || !IntersectsSolidItem(actor, center);
        }

        private bool IsInsideWorld(WorldActor actor, WorldPoint center)
        {
            double halfWidth = actor.Width / 2.0;
            double halfHeight = actor.Height / 2.0;
            return center.X - halfWidth >= 0 &&
                   center.Y - halfHeight >= 0 &&
                   center.X + halfWidth <= Width &&
                   center.Y + halfHeight <= Height;
        }

        private bool IntersectsSolidItem(WorldActor actor, WorldPoint center)
        {
            double ax1 = center.X - actor.Width / 2.0;
            double ay1 = center.Y - actor.Height / 2.0;
            double ax2 = center.X + actor.Width / 2.0;
            double ay2 = center.Y + actor.Height / 2.0;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (!item.Solid) continue;

                double bx1 = item.Position.X;
                double by1 = item.Position.Y;
                double bx2 = item.Position.X + item.Width;
                double by2 = item.Position.Y + item.Height;

                if (ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1)
                    return true;
            }

            return false;
        }

        private static bool Contains(WorldItem item, WorldPoint point)
        {
            return point.X >= item.Position.X &&
                   point.X <= item.Position.X + item.Width &&
                   point.Y >= item.Position.Y &&
                   point.Y <= item.Position.Y + item.Height;
        }
    }
}

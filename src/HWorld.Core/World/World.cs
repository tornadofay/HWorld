using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class World
    {
        private readonly List<WorldItem> _items = new List<WorldItem>();
        private readonly List<WorldActor> _actors = new List<WorldActor>();
        private readonly WorldSpatialIndex _spatialIndex;
        private readonly List<WorldItem> _queryBuffer = new List<WorldItem>(32);

        public World(double width, double height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            _spatialIndex = new WorldSpatialIndex(width, height);
        }

        public double Width { get; }
        public double Height { get; }
        public double SimulationTime { get; private set; }
        public IReadOnlyList<WorldItem> Items => _items;
        public IReadOnlyList<WorldActor> Actors => _actors;
        public WorldSpatialIndex SpatialIndex => _spatialIndex;

        public WorldItem AddItem(WorldPoint position, double width = 1, double height = 1, bool solid = false)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            var item = new WorldItem(Guid.NewGuid(), position) { Width = width, Height = height, Solid = solid };
            _items.Add(item);
            _spatialIndex.Add(item);
            return item;
        }

        internal WorldItem RestoreItem(WorldItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Width <= 0) throw new ArgumentOutOfRangeException(nameof(item));
            if (item.Height <= 0) throw new ArgumentOutOfRangeException(nameof(item));
            _items.Add(item);
            _spatialIndex.Add(item);
            return item;
        }

        public bool RemoveItem(Guid id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id != id) continue;
                _items.RemoveAt(i);
                _spatialIndex.Remove(id);
                return true;
            }
            return false;
        }

        public void NotifyItemChanged(WorldItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _spatialIndex.Update(item);
        }

        public WorldActor AddActor(WorldPoint position, double width = 1.6, double height = 1.6, double speed = 5.0)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (speed < 0) throw new ArgumentOutOfRangeException(nameof(speed));
            var actor = new WorldActor(Guid.NewGuid(), position) { Width = width, Height = height, Speed = speed };
            if (!IsInsideWorld(actor, position)) throw new ArgumentOutOfRangeException(nameof(position));
            if (actor.Collides && IntersectsSolidItem(actor, position)) throw new InvalidOperationException("The actor cannot be spawned inside a solid world item.");
            if (actor.Collides && IntersectsCollidingActor(actor, position)) throw new InvalidOperationException("The actor cannot be spawned inside another colliding actor.");
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
            if (actor.Collides && IntersectsSolidItem(actor, actor.Position)) throw new InvalidOperationException("The restored actor cannot occupy a solid world item.");
            if (actor.Collides && IntersectsCollidingActor(actor, actor.Position)) throw new InvalidOperationException("The restored actor cannot overlap another colliding actor.");
            _actors.Add(actor);
            return actor;
        }

        public bool RemoveActor(Guid id)
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i].Id != id) continue;
                _actors.RemoveAt(i);
                return true;
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
            var xTarget = new WorldPoint(target.X, actor.Position.Y);
            if (CanOccupy(actor, xTarget)) { actor.Position = xTarget; moved = Math.Abs(deltaX) > 0.000001; }
            var yTarget = new WorldPoint(actor.Position.X, target.Y);
            if (CanOccupy(actor, yTarget)) { actor.Position = yTarget; moved = moved || Math.Abs(deltaY) > 0.000001; }
            if (moved) actor.RotationDegrees = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            return moved;
        }

        public void EnqueueMove(Guid actorId, double directionX, double directionY, double durationSeconds)
        {
            if (durationSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            var actor = FindRequiredActor(actorId);
            if (Math.Abs(directionX) < 0.000001 && Math.Abs(directionY) < 0.000001) throw new ArgumentException("A move direction cannot be zero.");
            actor.EnqueueAction(new WorldActorAction(WorldActorActionKind.Move, directionX, directionY, durationSeconds));
        }

        public void EnqueueTurn(Guid actorId, double deltaDegrees)
        {
            var actor = FindRequiredActor(actorId);
            actor.EnqueueAction(new WorldActorAction(WorldActorActionKind.Turn, deltaDegrees, 0, 0));
        }

        public void EnqueueWait(Guid actorId, double durationSeconds)
        {
            if (durationSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            var actor = FindRequiredActor(actorId);
            actor.EnqueueAction(new WorldActorAction(WorldActorActionKind.Wait, 0, 0, durationSeconds));
        }

        public WorldActor FindActor(Guid id)
        {
            for (int i = 0; i < _actors.Count; i++) if (_actors[i].Id == id) return _actors[i];
            return null;
        }

        public WorldItem FindItemAt(WorldPoint point)
        {
            if (!_spatialIndex.TryGetItemsAt(point, _queryBuffer)) return null;
            for (int i = _queryBuffer.Count - 1; i >= 0; i--)
            {
                var item = _queryBuffer[i];
                if (Contains(item, point)) return item;
            }
            return null;
        }

        public WorldItem FindNearestInteractable(WorldPoint point, double reach = 2.5)
        {
            if (reach < 0) throw new ArgumentOutOfRangeException(nameof(reach));
            var minX = point.X - reach;
            var minY = point.Y - reach;
            var maxX = point.X + reach;
            var maxY = point.Y + reach;
            _spatialIndex.Query(new WorldPoint(minX, minY), new WorldPoint(maxX, maxY), _queryBuffer);

            WorldItem best = null;
            var bestDistanceSquared = reach * reach;
            for (int i = 0; i < _queryBuffer.Count; i++)
            {
                var item = _queryBuffer[i];
                if (!item.Interactable) continue;
                var centerX = item.Position.X + item.Width * 0.5;
                var centerY = item.Position.Y + item.Height * 0.5;
                var dx = centerX - point.X;
                var dy = centerY - point.Y;
                var distanceSquared = dx * dx + dy * dy;
                if (distanceSquared <= bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    best = item;
                }
            }
            return best;
        }

        public void Update(double deltaSeconds)
        {
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            for (int i = 0; i < _actors.Count; i++)
            {
                var actor = _actors[i];
                if (actor.Controller == null || actor.IsActionActive || actor.PendingActionCount > 0) continue;
                actor.Controller.Update(new WorldActorControllerContext(this, actor, deltaSeconds));
            }

            for (int i = 0; i < _actors.Count; i++)
            {
                var actor = _actors[i];
                if (!actor.TryStartNextAction()) continue;

                var action = actor.ActiveAction;
                switch (action.Kind)
                {
                    case WorldActorActionKind.Move:
                        var dx = action.X;
                        var dy = action.Y;
                        var directionLength = Math.Sqrt(dx * dx + dy * dy);
                        if (directionLength > 0.000001)
                        {
                            dx /= directionLength;
                            dy /= directionLength;
                            MoveActor(actor.Id, dx * deltaSeconds, dy * deltaSeconds, deltaSeconds);
                        }
                        actor.ConsumeActionTime(deltaSeconds);
                        break;
                    case WorldActorActionKind.Turn:
                        actor.RotationDegrees += action.X;
                        actor.ConsumeActionTime(action.DurationSeconds <= 0 ? 0.000001 : action.DurationSeconds);
                        break;
                    case WorldActorActionKind.Wait:
                        actor.ConsumeActionTime(deltaSeconds);
                        break;
                }
            }

            SimulationTime += deltaSeconds;
        }

        internal void SetSimulationTime(double value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            SimulationTime = value;
        }

        private WorldActor FindRequiredActor(Guid id)
        {
            var actor = FindActor(id);
            if (actor == null) throw new ArgumentException("Unknown actor.", nameof(id));
            return actor;
        }

        private bool CanOccupy(WorldActor actor, WorldPoint center)
        {
            if (!IsInsideWorld(actor, center)) return false;
            if (!actor.Collides) return true;
            return !IntersectsSolidItem(actor, center) && !IntersectsCollidingActor(actor, center);
        }

        private bool IsInsideWorld(WorldActor actor, WorldPoint center)
        {
            double halfWidth = actor.Width / 2.0;
            double halfHeight = actor.Height / 2.0;
            return center.X - halfWidth >= 0 && center.Y - halfHeight >= 0 && center.X + halfWidth <= Width && center.Y + halfHeight <= Height;
        }

        private bool IntersectsSolidItem(WorldActor actor, WorldPoint center)
        {
            double minX = center.X - actor.Width / 2.0;
            double minY = center.Y - actor.Height / 2.0;
            double maxX = center.X + actor.Width / 2.0;
            double maxY = center.Y + actor.Height / 2.0;
            _spatialIndex.Query(new WorldPoint(minX, minY), new WorldPoint(maxX, maxY), _queryBuffer);
            for (int i = 0; i < _queryBuffer.Count; i++)
            {
                var item = _queryBuffer[i];
                if (!item.Solid) continue;
                double bx1 = item.Position.X;
                double by1 = item.Position.Y;
                double bx2 = item.Position.X + item.Width;
                double by2 = item.Position.Y + item.Height;
                if (minX < bx2 && maxX > bx1 && minY < by2 && maxY > by1) return true;
            }
            return false;
        }

        private bool IntersectsCollidingActor(WorldActor actor, WorldPoint center)
        {
            double minX = center.X - actor.Width / 2.0;
            double minY = center.Y - actor.Height / 2.0;
            double maxX = center.X + actor.Width / 2.0;
            double maxY = center.Y + actor.Height / 2.0;

            for (int i = 0; i < _actors.Count; i++)
            {
                var other = _actors[i];
                if (other.Id == actor.Id || !other.Collides) continue;
                double bx1 = other.Position.X - other.Width / 2.0;
                double by1 = other.Position.Y - other.Height / 2.0;
                double bx2 = other.Position.X + other.Width / 2.0;
                double by2 = other.Position.Y + other.Height / 2.0;
                if (minX < bx2 && maxX > bx1 && minY < by2 && maxY > by1) return true;
            }
            return false;
        }

        private static bool Contains(WorldItem item, WorldPoint point)
        {
            return point.X >= item.Position.X && point.X <= item.Position.X + item.Width && point.Y >= item.Position.Y && point.Y <= item.Position.Y + item.Height;
        }
    }
}

using System;
using System.Collections.Generic;
using HWorld.Core.World;

namespace HWorld.Core.Geometry
{
    /// <summary>
    /// Renderer-independent 2D field-of-view sensor. It reports geometric
    /// observations without exposing application-defined names or kinds.
    /// A camera instance owns reusable candidate storage for repeated Observe calls.
    /// </summary>
    public sealed class WorldGeometryCamera
    {
        private readonly List<WorldItem> _candidateBuffer = new List<WorldItem>(32);
        private readonly List<WorldActor> _actorBuffer = new List<WorldActor>(8);

        public WorldGeometryCamera(double range = 50.0, double fieldOfViewDegrees = 90.0)
        {
            Range = range;
            FieldOfViewDegrees = fieldOfViewDegrees;
        }

        public double Range { get; set; }
        public double FieldOfViewDegrees { get; set; }
        public bool IncludeSolidState { get; set; } = true;
        public bool IncludeActors { get; set; } = true;

        /// <summary>
        /// Fills the supplied observation buffer. The buffer is cleared first.
        /// This camera reuses internal candidate storage and is therefore intended
        /// to be owned by one execution context; it is not thread-safe.
        /// </summary>
        public int Observe(HWorld.Core.World.World world, WorldActor observer, IList<WorldGeometryObservation> observations)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (Range <= 0) throw new InvalidOperationException("Range must be greater than zero.");
            if (FieldOfViewDegrees <= 0 || FieldOfViewDegrees > 360) throw new InvalidOperationException("Field of view must be between 0 and 360 degrees.");

            observations.Clear();
            var r = Range;
            world.SpatialIndex.Query(
                new WorldPoint(observer.Position.X - r, observer.Position.Y - r),
                new WorldPoint(observer.Position.X + r, observer.Position.Y + r),
                _candidateBuffer);

            double halfFov = FieldOfViewDegrees * 0.5;
            for (int i = 0; i < _candidateBuffer.Count; i++)
            {
                var item = _candidateBuffer[i];
                AddObservation(observations, observer, item.Id,
                    item.Position.X + item.Width * 0.5,
                    item.Position.Y + item.Height * 0.5,
                    item.Width, item.Height, item.RotationDegrees, IncludeSolidState && item.Solid, halfFov);
            }

            if (IncludeActors)
            {
                _actorBuffer.Clear();
                for (int i = 0; i < world.Actors.Count; i++)
                {
                    var actor = world.Actors[i];
                    if (actor.Id != observer.Id) _actorBuffer.Add(actor);
                }

                for (int i = 0; i < _actorBuffer.Count; i++)
                {
                    var actor = _actorBuffer[i];
                    AddObservation(observations, observer, actor.Id,
                        actor.Position.X, actor.Position.Y,
                        actor.Width, actor.Height, actor.RotationDegrees, false, halfFov);
                }
            }

            return observations.Count;
        }

        private static void AddObservation(
            IList<WorldGeometryObservation> observations,
            WorldActor observer,
            Guid entityId,
            double centerX,
            double centerY,
            double width,
            double height,
            double rotationDegrees,
            bool solid,
            double halfFov)
        {
            var dx = centerX - observer.Position.X;
            var dy = centerY - observer.Position.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance <= 0.000001 || distance > double.MaxValue) return;

            var bearing = NormalizeAngle(Math.Atan2(dy, dx) * 180.0 / Math.PI - observer.RotationDegrees);
            if (Math.Abs(bearing) > halfFov) return;

            observations.Add(new WorldGeometryObservation(
                entityId,
                dx,
                dy,
                distance,
                bearing,
                width,
                height,
                rotationDegrees,
                solid));
        }

        private static double NormalizeAngle(double degrees)
        {
            while (degrees <= -180.0) degrees += 360.0;
            while (degrees > 180.0) degrees -= 360.0;
            return degrees;
        }
    }
}

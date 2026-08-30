using System;
using System.Collections.Generic;
using HWorld.Core.World;

namespace HWorld.Core.Geometry
{
    /// <summary>
    /// Renderer-independent 2D field-of-view sensor. It reports geometric
    /// observations without exposing application-defined names or kinds.
    /// </summary>
    public sealed class WorldGeometryCamera
    {
        public WorldGeometryCamera(double range = 50.0, double fieldOfViewDegrees = 90.0)
        {
            Range = range;
            FieldOfViewDegrees = fieldOfViewDegrees;
        }

        public double Range { get; set; }
        public double FieldOfViewDegrees { get; set; }
        public bool IncludeSolidState { get; set; } = true;

        /// <summary>
        /// Fills the supplied observation buffer. The buffer is cleared first.
        /// </summary>
        public int Observe(World world, WorldActor observer, IList<WorldGeometryObservation> observations)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (Range <= 0) throw new InvalidOperationException("Range must be greater than zero.");
            if (FieldOfViewDegrees <= 0 || FieldOfViewDegrees > 360) throw new InvalidOperationException("Field of view must be between 0 and 360 degrees.");

            observations.Clear();
            var candidates = new List<WorldItem>(32);
            var r = Range;
            world.SpatialIndex.Query(
                new WorldPoint(observer.Position.X - r, observer.Position.Y - r),
                new WorldPoint(observer.Position.X + r, observer.Position.Y + r),
                candidates);

            double halfFov = FieldOfViewDegrees * 0.5;
            for (int i = 0; i < candidates.Count; i++)
            {
                var item = candidates[i];
                var centerX = item.Position.X + item.Width * 0.5;
                var centerY = item.Position.Y + item.Height * 0.5;
                var dx = centerX - observer.Position.X;
                var dy = centerY - observer.Position.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance <= 0.000001 || distance > Range) continue;

                var bearing = NormalizeAngle(Math.Atan2(dy, dx) * 180.0 / Math.PI - observer.RotationDegrees);
                if (Math.Abs(bearing) > halfFov) continue;

                observations.Add(new WorldGeometryObservation(
                    item.Id,
                    dx,
                    dy,
                    distance,
                    bearing,
                    item.Width,
                    item.Height,
                    item.RotationDegrees,
                    IncludeSolidState && item.Solid));
            }

            return observations.Count;
        }

        private static double NormalizeAngle(double degrees)
        {
            while (degrees <= -180.0) degrees += 360.0;
            while (degrees > 180.0) degrees -= 360.0;
            return degrees;
        }
    }
}

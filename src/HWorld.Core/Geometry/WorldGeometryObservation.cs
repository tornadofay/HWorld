using System;

namespace HWorld.Core.Geometry
{
    /// <summary>
    /// Compact geometric observation of a world item relative to an observer.
    /// It intentionally contains no semantic name or application-specific kind.
    /// </summary>
    public readonly struct WorldGeometryObservation
    {
        public WorldGeometryObservation(
            Guid entityId,
            double relativeX,
            double relativeY,
            double distance,
            double bearingDegrees,
            double width,
            double height,
            double rotationDegrees,
            bool solid)
        {
            EntityId = entityId;
            RelativeX = relativeX;
            RelativeY = relativeY;
            Distance = distance;
            BearingDegrees = bearingDegrees;
            Width = width;
            Height = height;
            RotationDegrees = rotationDegrees;
            Solid = solid;
        }

        public Guid EntityId { get; }
        public double RelativeX { get; }
        public double RelativeY { get; }
        public double Distance { get; }
        public double BearingDegrees { get; }
        public double Width { get; }
        public double Height { get; }
        public double RotationDegrees { get; }
        public bool Solid { get; }
    }
}

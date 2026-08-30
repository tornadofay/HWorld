namespace HWorld.Core.World
{
    public enum WorldActorActionKind
    {
        Move,
        Turn,
        Wait
    }

    /// <summary>
    /// A small deterministic action request executed by the world simulation.
    /// Directions are interpreted as world-space components and normalized by movement execution.
    /// </summary>
    public sealed class WorldActorAction
    {
        public WorldActorAction(WorldActorActionKind kind, double x, double y, double durationSeconds)
        {
            Kind = kind;
            X = x;
            Y = y;
            DurationSeconds = durationSeconds;
        }

        public WorldActorActionKind Kind { get; }
        public double X { get; }
        public double Y { get; }
        public double DurationSeconds { get; }
    }
}
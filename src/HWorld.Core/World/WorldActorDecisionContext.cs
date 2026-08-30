using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    /// <summary>
    /// Immutable state captured on the simulation thread and supplied to an asynchronous
    /// decision provider. It prevents background decision work from reading mutable world state.
    /// </summary>
    public sealed class WorldActorDecisionContext
    {
        public WorldActorDecisionContext(
            WorldActor actor,
            double simulationTime,
            string observation)
        {
            ActorId = actor.Id;
            Position = actor.Position;
            RotationDegrees = actor.RotationDegrees;
            Width = actor.Width;
            Height = actor.Height;
            Speed = actor.Speed;
            SimulationTime = simulationTime;
            Observation = observation ?? string.Empty;
        }

        public System.Guid ActorId { get; }
        public WorldPoint Position { get; }
        public double RotationDegrees { get; }
        public double Width { get; }
        public double Height { get; }
        public double Speed { get; }
        public double SimulationTime { get; }
        public string Observation { get; }
    }
}

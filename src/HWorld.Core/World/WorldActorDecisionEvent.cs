using System;

namespace HWorld.Core.World
{
    public enum WorldActorDecisionOutcome
    {
        Started,
        Completed,
        TimedOut,
        Cancelled,
        Failed,
        Rejected
    }

    public sealed class WorldActorDecisionEvent
    {
        public WorldActorDecisionEvent(Guid requestId, Guid actorId, WorldActorDecisionOutcome outcome, double simulationTime, double elapsedSeconds, string error)
        {
            RequestId = requestId;
            ActorId = actorId;
            Outcome = outcome;
            SimulationTime = simulationTime;
            ElapsedSeconds = elapsedSeconds;
            Error = error ?? string.Empty;
        }

        public Guid RequestId { get; }
        public Guid ActorId { get; }
        public WorldActorDecisionOutcome Outcome { get; }
        public double SimulationTime { get; }
        public double ElapsedSeconds { get; }
        public string Error { get; }
    }
}

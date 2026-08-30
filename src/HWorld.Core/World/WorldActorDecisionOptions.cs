using System;

namespace HWorld.Core.World
{
    public enum WorldDecisionSchedulingMode
    {
        Asynchronous,
        DeterministicCheckpoint
    }

    /// <summary>
    /// Scheduling policy for one asynchronously deciding actor.
    /// Cadence is measured in simulation seconds. Timeout can be measured by wall clock
    /// for realistic latency experiments or by simulation time for controlled experiments.
    /// </summary>
    public sealed class WorldActorDecisionOptions
    {
        public double DecisionCadenceSeconds { get; set; } = 0.5;
        public TimeSpan DecisionTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public WorldDecisionSchedulingMode SchedulingMode { get; set; } = WorldDecisionSchedulingMode.Asynchronous;
        public bool StartImmediately { get; set; } = true;
    }
}

using System;

namespace HWorld.Core.World
{
    /// <summary>
    /// Restricted world-facing context supplied to one actor controller per update.
    /// </summary>
    public sealed class WorldActorControllerContext
    {
        internal WorldActorControllerContext(World world, WorldActor actor, double deltaSeconds)
        {
            World = world;
            Actor = actor;
            DeltaSeconds = deltaSeconds;
        }

        public World World { get; }
        public WorldActor Actor { get; }
        public double DeltaSeconds { get; }
        public bool IsBusy => Actor.IsActionActive || Actor.PendingActionCount > 0;

        public void Move(double directionX, double directionY, double durationSeconds)
        {
            World.EnqueueMove(Actor.Id, directionX, directionY, durationSeconds);
        }

        public void Turn(double deltaDegrees)
        {
            World.EnqueueTurn(Actor.Id, deltaDegrees);
        }

        public void Wait(double durationSeconds)
        {
            World.EnqueueWait(Actor.Id, durationSeconds);
        }
    }
}
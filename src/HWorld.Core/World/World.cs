using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed partial class World
    {
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
                            MoveActor(actor.Id, dx * actor.Speed * deltaSeconds, dy * actor.Speed * deltaSeconds, deltaSeconds);
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
    }
}
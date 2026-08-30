using System;
using System.Collections.Generic;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public sealed class WorldActor
    {
        private readonly Queue<WorldActorAction> _actionQueue = new Queue<WorldActorAction>();
        private WorldActorAction _activeAction;
        private double _activeActionRemainingSeconds;

        public WorldActor(Guid id, WorldPoint position)
        {
            Id = id;
            Position = position;
        }

        public Guid Id { get; }
        public WorldPoint Position { get; internal set; }
        public double RotationDegrees { get; set; }
        public double Width { get; set; } = 1.0;
        public double Height { get; set; } = 1.0;
        public double Speed { get; set; } = 5.0;
        public bool Collides { get; set; } = true;
        public string Name { get; set; } = "Actor";

        /// <summary>Optional non-LLM controller invoked by the world when this actor is idle.</summary>
        public IWorldActorController Controller { get; set; }

        /// <summary>Number of queued or currently executing actions for this actor.</summary>
        public int PendingActionCount => _actionQueue.Count + (_activeAction != null ? 1 : 0);

        /// <summary>True while an action is currently being executed.</summary>
        public bool IsActionActive => _activeAction != null;

        internal bool TryStartNextAction()
        {
            if (_activeAction != null || _actionQueue.Count == 0) return false;
            _activeAction = _actionQueue.Dequeue();
            _activeActionRemainingSeconds = _activeAction.DurationSeconds;
            return true;
        }

        internal WorldActorAction ActiveAction => _activeAction;

        internal double ActiveActionRemainingSeconds => _activeActionRemainingSeconds;

        internal void ConsumeActionTime(double seconds)
        {
            if (_activeAction == null) return;
            _activeActionRemainingSeconds -= seconds;
            if (_activeActionRemainingSeconds <= 0.000001)
            {
                _activeAction = null;
                _activeActionRemainingSeconds = 0;
            }
        }

        internal void EnqueueAction(WorldActorAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _actionQueue.Enqueue(action);
        }
    }
}
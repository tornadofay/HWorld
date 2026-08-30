using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HWorld.Core.World
{
    public sealed class WorldActorDecisionScheduler : IDisposable
    {
        private sealed class Slot
        {
            public readonly WorldActor Actor;
            public readonly IWorldActorDecisionProvider Provider;
            public readonly WorldActorDecisionOptions Options;
            public double NextDecisionSimulationTime;
            public bool ActionCompleted;
            public Request Active;

            public Slot(WorldActor actor, IWorldActorDecisionProvider provider, WorldActorDecisionOptions options)
            {
                Actor = actor;
                Provider = provider;
                Options = options;
                NextDecisionSimulationTime = options.StartImmediately ? 0 : options.DecisionCadenceSeconds;
            }
        }

        private sealed class Request
        {
            public readonly Guid Id;
            public readonly Slot Slot;
            public readonly CancellationTokenSource Cancellation;
            public readonly Task<WorldActorAction> Task;
            public readonly double StartedSimulationTime;
            public readonly long StartedTimestamp;

            public Request(Guid id, Slot slot, CancellationTokenSource cancellation, Task<WorldActorAction> task, double startedSimulationTime)
            {
                Id = id;
                Slot = slot;
                Cancellation = cancellation;
                Task = task;
                StartedSimulationTime = startedSimulationTime;
                StartedTimestamp = Stopwatch.GetTimestamp();
            }
        }

        private readonly World _world;
        private readonly Dictionary<Guid, Slot> _slots = new Dictionary<Guid, Slot>();
        private readonly List<Request> _retiredRequests = new List<Request>();
        private bool _disposed;

        public WorldActorDecisionScheduler(World world, int maxConcurrentRequests = 4)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            if (maxConcurrentRequests <= 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrentRequests));
            MaxConcurrentRequests = maxConcurrentRequests;
        }

        public int MaxConcurrentRequests { get; }
        public int ActiveRequestCount { get; private set; }
        public Func<WorldActor, string> ObservationFactory { get; set; }
        public event EventHandler<WorldActorDecisionEvent> DecisionLifecycle;

        public void Register(WorldActor actor, IWorldActorDecisionProvider provider, WorldActorDecisionOptions options = null)
        {
            ThrowIfDisposed();
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (actor.Controller != null) throw new InvalidOperationException("An actor using the asynchronous decision scheduler cannot also use a synchronous controller.");
            if (_slots.ContainsKey(actor.Id)) throw new InvalidOperationException("The actor is already registered with this scheduler.");

            options = CloneAndValidate(options ?? new WorldActorDecisionOptions());
            _slots.Add(actor.Id, new Slot(actor, provider, options));
            actor.ActionCompleted += OnActorActionCompleted;
        }

        public bool Unregister(Guid actorId)
        {
            ThrowIfDisposed();
            Slot slot;
            if (!_slots.TryGetValue(actorId, out slot)) return false;

            CancelActive(slot, WorldActorDecisionOutcome.Cancelled, "Actor scheduling was unregistered.");
            slot.Actor.ActionCompleted -= OnActorActionCompleted;
            _slots.Remove(actorId);
            return true;
        }

        public void Update(double simulationTime)
        {
            ThrowIfDisposed();
            if (simulationTime < 0) throw new ArgumentOutOfRangeException(nameof(simulationTime));

            CleanupRetiredRequests();
            var slots = new List<Slot>(_slots.Values);
            for (int i = 0; i < slots.Count; i++) PollRequest(slots[i], simulationTime);

            int available = MaxConcurrentRequests - ActiveRequestCount;
            if (available <= 0) return;

            for (int i = 0; i < slots.Count && available > 0; i++)
            {
                var slot = slots[i];
                if (slot.ActionCompleted)
                {
                    slot.ActionCompleted = false;
                    if (slot.Active == null && slot.Actor.PendingActionCount == 0)
                        slot.NextDecisionSimulationTime = simulationTime;
                }

                if (slot.Active != null || slot.Actor.PendingActionCount > 0) continue;
                if (simulationTime + 0.000001 < slot.NextDecisionSimulationTime) continue;

                StartDecision(slot, simulationTime);
                available--;
            }
        }

        public void CancelAll()
        {
            ThrowIfDisposed();
            var slots = new List<Slot>(_slots.Values);
            for (int i = 0; i < slots.Count; i++) CancelActive(slots[i], WorldActorDecisionOutcome.Cancelled, "All actor decisions were cancelled.");
        }

        private void StartDecision(Slot slot, double simulationTime)
        {
            string observation = ObservationFactory == null ? string.Empty : ObservationFactory(slot.Actor) ?? string.Empty;
            var context = new WorldActorDecisionContext(slot.Actor, simulationTime, observation);
            var cancellation = new CancellationTokenSource();
            var requestId = Guid.NewGuid();

            Task<WorldActorAction> task;
            try
            {
                task = slot.Provider.DecideAsync(context, cancellation.Token);
                if (task == null) throw new InvalidOperationException("Decision provider returned a null task.");
            }
            catch (Exception ex)
            {
                cancellation.Dispose();
                slot.NextDecisionSimulationTime = simulationTime + slot.Options.DecisionCadenceSeconds;
                Raise(new WorldActorDecisionEvent(requestId, slot.Actor.Id, WorldActorDecisionOutcome.Failed, simulationTime, 0, ex.Message));
                return;
            }

            var request = new Request(requestId, slot, cancellation, task, simulationTime);
            slot.Active = request;
            ActiveRequestCount++;
            Raise(new WorldActorDecisionEvent(requestId, slot.Actor.Id, WorldActorDecisionOutcome.Started, simulationTime, 0, string.Empty));
        }

        private void PollRequest(Slot slot, double simulationTime)
        {
            var request = slot.Active;
            if (request == null) return;

            if (!request.Task.IsCompleted)
            {
                if (HasTimedOut(request, simulationTime))
                {
                    request.Cancellation.Cancel();
                    slot.Active = null;
                    ActiveRequestCount--;
                    slot.NextDecisionSimulationTime = simulationTime + slot.Options.DecisionCadenceSeconds;
                    _retiredRequests.Add(request);
                    Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.TimedOut, simulationTime, GetElapsedSeconds(request), "Decision timeout."));
                }
                return;
            }

            slot.Active = null;
            ActiveRequestCount--;
            slot.NextDecisionSimulationTime = simulationTime + slot.Options.DecisionCadenceSeconds;
            _retiredRequests.Add(request);

            WorldActorAction action;
            try
            {
                action = request.Task.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Cancelled, simulationTime, GetElapsedSeconds(request), "Decision cancelled."));
                return;
            }
            catch (Exception ex)
            {
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Failed, simulationTime, GetElapsedSeconds(request), ex.Message));
                return;
            }

            if (action == null)
            {
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Rejected, simulationTime, GetElapsedSeconds(request), "Decision provider returned no action."));
                return;
            }

            if (slot.Actor.PendingActionCount > 0)
            {
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Rejected, simulationTime, GetElapsedSeconds(request), "Decision result arrived after the actor became busy."));
                return;
            }

            try
            {
                EnqueueValidatedAction(slot.Actor.Id, action);
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Completed, simulationTime, GetElapsedSeconds(request), string.Empty));
            }
            catch (Exception ex)
            {
                Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, WorldActorDecisionOutcome.Rejected, simulationTime, GetElapsedSeconds(request), ex.Message));
            }
        }

        private bool HasTimedOut(Request request, double simulationTime)
        {
            if (request.Slot.Options.DecisionTimeout == Timeout.InfiniteTimeSpan) return false;
            if (request.Slot.Options.SchedulingMode == WorldDecisionSchedulingMode.DeterministicCheckpoint)
                return simulationTime - request.StartedSimulationTime >= request.Slot.Options.DecisionTimeout.TotalSeconds;
            return GetElapsedSeconds(request) >= request.Slot.Options.DecisionTimeout.TotalSeconds;
        }

        private static double GetElapsedSeconds(Request request)
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - request.StartedTimestamp;
            return elapsedTicks / (double)Stopwatch.Frequency;
        }

        private void EnqueueValidatedAction(Guid actorId, WorldActorAction action)
        {
            switch (action.Kind)
            {
                case WorldActorActionKind.Move:
                    if (action.DurationSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(action));
                    if (Math.Abs(action.X) < 0.000001 && Math.Abs(action.Y) < 0.000001) throw new ArgumentException("A move direction cannot be zero.", nameof(action));
                    _world.EnqueueMove(actorId, action.X, action.Y, action.DurationSeconds);
                    break;
                case WorldActorActionKind.Turn:
                    _world.EnqueueTurn(actorId, action.X);
                    break;
                case WorldActorActionKind.Wait:
                    if (action.DurationSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(action));
                    _world.EnqueueWait(actorId, action.DurationSeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        private void OnActorActionCompleted(object sender, EventArgs e)
        {
            var actor = sender as WorldActor;
            if (actor == null) return;
            Slot slot;
            if (_slots.TryGetValue(actor.Id, out slot)) slot.ActionCompleted = true;
        }

        private void CancelActive(Slot slot, WorldActorDecisionOutcome outcome, string message)
        {
            var request = slot.Active;
            if (request == null) return;
            request.Cancellation.Cancel();
            slot.Active = null;
            ActiveRequestCount--;
            _retiredRequests.Add(request);
            Raise(new WorldActorDecisionEvent(request.Id, slot.Actor.Id, outcome, _world.SimulationTime, GetElapsedSeconds(request), message));
        }

        private void CleanupRetiredRequests()
        {
            for (int i = _retiredRequests.Count - 1; i >= 0; i--)
            {
                var request = _retiredRequests[i];
                if (!request.Task.IsCompleted) continue;
                request.Cancellation.Dispose();
                _retiredRequests.RemoveAt(i);
            }
        }

        private static WorldActorDecisionOptions CloneAndValidate(WorldActorDecisionOptions source)
        {
            if (source.DecisionCadenceSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(source.DecisionCadenceSeconds));
            if (source.DecisionTimeout != Timeout.InfiniteTimeSpan && source.DecisionTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(source.DecisionTimeout));
            return new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = source.DecisionCadenceSeconds,
                DecisionTimeout = source.DecisionTimeout,
                SchedulingMode = source.SchedulingMode,
                StartImmediately = source.StartImmediately
            };
        }

        private void Raise(WorldActorDecisionEvent e)
        {
            DecisionLifecycle?.Invoke(this, e);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WorldActorDecisionScheduler));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var pair in _slots)
            {
                var slot = pair.Value;
                if (slot.Active != null) slot.Active.Cancellation.Cancel();
                slot.Actor.ActionCompleted -= OnActorActionCompleted;
            }

            for (int i = 0; i < _retiredRequests.Count; i++)
            {
                var request = _retiredRequests[i];
                request.Cancellation.Cancel();
                if (request.Task.IsCompleted) request.Cancellation.Dispose();
            }

            _slots.Clear();
            _retiredRequests.Clear();
            ActiveRequestCount = 0;
        }
    }
}

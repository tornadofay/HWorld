# Decision Scheduling Design

## Purpose

Phase 4 separates actor decision latency from continuous HWorld simulation time.

The world must continue advancing while an external decision provider is working. Decision providers never mutate the world directly. They receive an immutable actor snapshot and return a decision asynchronously.

## Scheduler boundary

`WorldActorDecisionScheduler` owns the lifecycle between an actor and an external decision provider:

```text
simulation thread
    |
    +-- capture actor state + observation
    |
    +-- start async decision request
    |
    +-- world continues updating
    |
    +-- poll completion / timeout / cancellation
    |
    +-- validate result
    |
    +-- enqueue action on world
    |
    v
HWorld action execution
```

The provider never receives a mutable `World` or direct actor mutation API.

## Decision provider contract

`IWorldActorDecisionProvider` returns:

```csharp
Task<WorldActorAction> DecideAsync(
    WorldActorDecisionContext context,
    CancellationToken cancellationToken)
```

`WorldActorDecisionContext` is an immutable snapshot containing actor identity, physical state, simulation time, and an optional observation captured by the scheduler's `ObservationFactory` on the simulation thread.

The provider contract is generic. HWorld may implement it with a deterministic controller, an LLM/cognition library, a remote service, or another decision system.

## Per-actor scheduling

Each registered actor has independent:

- decision cadence in simulation seconds;
- decision timeout;
- start-immediately policy;
- scheduling mode;
- active request state.

Only one decision request may be active for a given actor at a time.

The scheduler also has a global maximum concurrent-request count so one population of actors cannot create unlimited outstanding work.

## Decision lifecycle

Every request has a unique `Guid` correlation ID. Lifecycle events report:

- Started
- Completed
- TimedOut
- Cancelled
- Failed
- Rejected

Completed and rejected events include measured decision latency where available.

A request that times out or is cancelled is retired. If its provider ignores cancellation and later completes, the scheduler no longer treats that request as active, so the late result cannot inject an action into the world.

## Action execution

The scheduler does not execute external cognition itself. It validates the returned decision using the HWorld action model and calls the existing world enqueue API:

```text
MOVE(direction, duration)
TURN(angle)
WAIT(duration)
```

The world remains authoritative over:

- movement speed;
- bounds;
- collision;
- action duration;
- simulation time;
- actor state.

An action result that arrives after the actor has become busy is rejected rather than appended blindly.

## Simulation time vs decision time

Simulation time is advanced by `World.Update` and is never blocked by provider latency.

Decision cadence is expressed in simulation seconds so experiments can control how often actors are eligible for new decisions.

Timeout measurement depends on scheduling mode:

### Asynchronous

Wall-clock time is measured with `Stopwatch`. This represents realistic external latency experiments.

### DeterministicCheckpoint

Timeouts are evaluated against simulation time at scheduler checkpoints. This supports controlled experiments where all decision application occurs at explicit simulation checkpoints.

In both modes, results are applied only by the simulation thread when `WorldActorDecisionScheduler.Update` is called.

## Action completion

`WorldActor.ActionCompleted` is raised when a current action finishes. The scheduler uses that signal to make an idle registered actor eligible for another decision at the next scheduler checkpoint instead of requiring a separate cognition tick for every action completion.

## Cancellation

`CancelAll` and `Unregister` cancel active requests. Provider implementations should observe the supplied `CancellationToken`.

Cancellation is advisory at the provider boundary; the world remains safe even when a provider ignores it because retired request IDs cannot re-enter the active scheduling path.

## Observation boundary

The scheduler does not invent an observation format. `ObservationFactory` receives the actor on the simulation thread and should call the actor's actual sensor/serializer pipeline. The resulting data is then frozen into the immutable decision context before asynchronous work starts.

This allows any external cognition implementation to consume the same authorized observation without moving perception into the decision scheduler.

## Example laboratory

`HWorld.Example` provides a Decision Scheduling Laboratory with two actors:

- one decision provider responds after 100 ms;
- another responds after 900 ms;
- both use the same continuous world;
- both produce validated movement actions;
- the world continues at 30 Hz;
- scheduler lifecycle and measured latency are visible.

The laboratory intentionally uses no external LLM or cognition library.

## Relationship to external cognition

Phase 4 introduces the generic asynchronous scheduling boundary only. It does not require a particular cognition library.

A future external cognition adapter can implement `IWorldActorDecisionProvider` and use the actor's authorized observations without changing the world simulation or scheduler contracts.

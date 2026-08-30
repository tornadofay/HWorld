# Agents and Simulation Time

## Continuous world

HWorld is not turn-based by default. `World.Update` advances simulation time independently of external decision latency.

## Phase 3 actor behavior

Each actor can own:

- physical state;
- an action queue;
- an optional non-LLM controller;
- an independent geometry sensor.

Controllers are deterministic behavior inputs. They may request validated actions but cannot directly mutate actor state or simulation time.

## Phase 4 asynchronous decisions

`WorldActorDecisionScheduler` separates decision/response time from simulation time.

A registered actor has its own:

- decision cadence;
- decision timeout;
- start-immediately policy;
- scheduling mode;
- active request state.

A scheduler-wide concurrency limit bounds the number of outstanding decisions.

Decision providers implement:

```text
IWorldActorDecisionProvider
    -> Task<WorldActorAction>
```

The provider receives an immutable `WorldActorDecisionContext` captured on the simulation thread. It never receives a mutable `World` reference.

## Decision lifecycle

Every decision request receives a correlation ID. Lifecycle outcomes are:

```text
Started
Completed
TimedOut
Cancelled
Failed
Rejected
```

Latency is measured for completed, failed, cancelled, rejected and timed-out requests where applicable.

## Slow agents

A slow decision does not freeze the world:

```text
world tick  -> decision starts
world tick  -> physical simulation continues
world tick  -> physical simulation continues
world tick  -> decision completes
world tick  -> returned action is validated and queued
```

The world thread never blocks waiting for a provider.

## Action model

The current pre-AI action subset is:

```text
MOVE(direction, duration)
TURN(angle)
WAIT(duration)
```

Action duration is physical execution time. Decision latency is external response time. They are deliberately separate quantities.

The world remains authoritative over movement speed, bounds, collision, action duration and actor state.

## Timeout and cancellation

In `Asynchronous` mode, timeout uses wall-clock time measured with `Stopwatch`, representing realistic provider latency.

In `DeterministicCheckpoint` mode, timeout uses simulation time, allowing controlled experiments to compare actors at synchronized simulation checkpoints.

Cancellation is advisory to providers. The scheduler retires cancelled or timed-out requests immediately. A provider that ignores cancellation may finish later, but its stale result cannot re-enter the active scheduling path.

## Action completion

Actor action completion raises `WorldActor.ActionCompleted`. The decision scheduler uses this signal to make an idle registered actor eligible for another decision at the next scheduler checkpoint.

This allows event-driven action completion without requiring cognition to run on every simulation tick.

## Decision application boundary

Asynchronous providers may finish on worker threads, but returned actions are never applied there.

```text
worker/provider thread
        |
        v
 decision result
        |
        v
simulation-thread scheduler checkpoint
        |
        v
validated world action queue
        |
        v
World.Update
```

This keeps the world state single-thread-authoritative.

## Observation boundary

`WorldActorDecisionScheduler.ObservationFactory` may create sensor output on the simulation thread. That output is frozen into the immutable decision context before asynchronous work starts.

The scheduler does not define semantics, memory, knowledge or reasoning. A future HAgent adapter can implement `IWorldActorDecisionProvider` without moving HAgent into HWorld.Core.

## Deterministic experiments

Phase 4 retains deterministic actor action execution order from Phase 3. Decision results are applied only at scheduler checkpoints, while scheduling mode controls timeout measurement.

The Example Decision Scheduling Laboratory uses two synthetic providers with intentionally different response latencies to make the separation observable without any LLM.

## Future fairness and cognition

Future work may add richer fairness controls, observation cadence, event-triggered wakeups and per-provider policies. Those concerns belong at the external decision/cognition boundary and must not make physical simulation dependent on model latency.

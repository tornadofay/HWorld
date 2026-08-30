# Agents and Simulation Time

## Continuous world

HWorld is not turn-based by default.

The simulation clock proceeds independently of external decision latency. Phase 3 establishes deterministic local actor behavior before asynchronous model scheduling is introduced.

## Phase 3 actor behavior

Each actor can now own:

- its own physical state
- its own action queue
- its own optional non-LLM controller
- its own geometry sensor instance

A controller is not an LLM integration. It is a deterministic behavior input that requests validated world actions such as `MOVE`, `TURN` and `WAIT`.

The world remains authoritative over physical execution. A controller cannot directly move an actor, modify simulation time, or bypass collision and bounds rules.

## Deterministic update order

During `World.Update(deltaSeconds)`:

1. actors are visited in actor-list order;
2. idle actors with controllers may enqueue their next action;
3. one active action is executed for each actor in that same order;
4. simulation time advances once for the world update.

This deterministic ordering is intended for repeatable experiments and will remain separate from the future asynchronous scheduling mode.

## Action model

The current pre-AI action subset is:

```text
MOVE(direction, duration)
TURN(angle)
WAIT(duration)
```

Movement directions are normalized before execution, and the actor's speed remains authoritative. Collisions with solid world items and other colliding actors are enforced by the world.

Future richer actions may include:

```text
LOOK()
INSPECT(objectId)
GRAB(objectId)
RELEASE()
USE(itemId, targetId)
DROP(itemId)
```

## Actor-specific perception

Each actor can own a separate geometry sensor with its own observation settings. Actor-to-actor perception uses the same anonymous geometry observation format as item perception and excludes the observing actor.

## Slow agents

Asynchronous model latency is not implemented in Phase 3. The world has no dependency on an LLM or provider, so a controller's execution cannot block the simulation clock.

Phase 4 introduces the actual separation between decision time and simulation time, including slow external decisions, action horizons, cancellation and timeout behavior.

## Event-driven cognition

Not every simulation tick should require an LLM.

Future external cognition may wake when events occur:

- novel object
- unexpected collision
- social encounter
- goal failure
- threat
- resource discovery
- contradiction in knowledge

Routine behavior can remain local and cheap.

## Decision fairness experiments

The framework should support both:

1. realistic asynchronous mode where external decision latency matters;
2. controlled experimental mode where actions are evaluated at synchronized simulation checkpoints.

Both modes should record timing information so results can be compared. This belongs to Phase 4 and later, not the Phase 3 deterministic controller layer.

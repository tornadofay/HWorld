# Agents and Simulation Time

## Continuous world

HWorld is not turn-based by default.

The simulation clock proceeds independently of LLM request latency.

## Agent cognition

Each agent can have its own:

- observation cadence
- decision cadence
- maximum concurrent thought requests
- timeout
- action horizon
- reasoning provider/model

Example:

```text
Agent A: observe 10 Hz, decide up to 4 Hz
Agent B: observe 5 Hz, decide up to 1 Hz
Agent C: event-driven decisions only
```

## Slow agents

If an agent takes 2 seconds to respond, the world does not freeze. Its previously committed action continues or expires according to action policy.

## Action model

Prefer high-level validated actions:

```text
MOVE(direction, duration)
TURN(angle)
LOOK()
INSPECT(objectId)
GRAB(objectId)
RELEASE()
USE(itemId, targetId)
DROP(itemId)
```

The engine validates reachability, collision, ownership, inventory and other constraints.

## Event-driven cognition

Not every tick should require an LLM.

Agents can wake an expensive reasoning process when events occur:

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

1. Realistic asynchronous mode where latency matters.
2. Controlled experimental mode where actions are evaluated at synchronized simulation checkpoints.

Both modes should record the timing information so results can be compared.

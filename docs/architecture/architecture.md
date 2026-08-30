# HWorld Architecture

## Boundary between HWorld and HAgent

HWorld owns the simulated reality.
HAgent owns model/provider/tool/agent execution concerns.

```text
+----------------------+       +----------------------+
| HWorld                |       | HAgent               |
|                      |       |                      |
| World                |       | Agent execution      |
| Time                 | <---> | Provider/model       |
| Physics              |       | Tool calling         |
| Collision            |       | Context construction |
| Spatial index         |       | Memory integrations  |
| Bodies/items          |       | Cancellation         |
| Sensors               |       | Execution results    |
| Actions               |       |                      |
| Rendering adapters    |       |                      |
+----------------------+       +----------------------+
```

HWorld should consume HAgent through a small adapter interface rather than referencing HAgent internals throughout the simulation.

## World layer

The world is authoritative.

It owns:

- entity existence
- identity
- positions and rotations
- item state
- physical properties
- interactions
- environmental state
- simulation time
- event sequencing

## Simulation clock

Simulation time is independent of wall-clock/API latency.

A decision request may be outstanding while the world continues to advance.

Actions therefore need timestamps or validity windows.

Example:

```text
T=10.000  Agent A observes
T=10.010  Agent B observes
T=10.080  A action arrives
T=11.700  B action arrives
```

The world remains valid throughout.

## Entities and items

Everything in the world has an identity, but identity does not imply semantic meaning to an agent.

```text
WorldEntity
  Id
  Transform
  Geometry
  PhysicalState
  InteractionState
  PerceptionProperties
  OptionalAgentBody
  OptionalItemProperties
```

An object may have an internal developer name such as `Object_17`, but semantic labels such as "tree" or "car" should not automatically be exposed to an agent's observation.

## Spatial index

Spatial indexing is an optimization and query facility, not a source of truth.

The initial implementation can use a simple grid or uniform partition. Later implementations may use a quadtree or another spatial index.

Queries should support:

- nearby entities
- entities intersecting an area
- ray/segment candidates
- camera/FOV candidates

## Physics and collision

Physics should remain intentionally simple in early versions. HWorld is an AI/artificial-life laboratory, not a replacement for a full game physics engine.

Start with:

- point/circle/rectangle geometry
- static obstacles
- basic movement constraints
- segment intersection
- overlap tests
- deterministic update order

## Renderer boundary

A renderer reads simulation state and draws it. It must not decide simulation outcomes.

A camera/sensor is conceptually separate from a renderer.

A GDI+ visual camera may use the same projection math as a geometry camera, then render that observation for the human observer.

## Example execution cycle

```text
1. Advance simulation time.
2. Apply scheduled/world actions.
3. Resolve physics and collisions.
4. Update sensors/cameras.
5. Produce observation snapshots.
6. Dispatch observations to eligible agents.
7. Receive actions asynchronously.
8. Validate actions against current world state.
9. Queue/apply valid actions according to action timing rules.
10. Record events for replay/analysis.
11. Render current state through the selected renderer.
```

This cycle can be implemented with different real-time speeds or in accelerated/headless mode.

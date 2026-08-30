# Multi-Actor Simulation Design

## Purpose

Phase 3 establishes multiple independently embodied actors in one authoritative HWorld simulation before any HAgent integration.

The world owns actor state, movement, collision, action execution and sensor-visible world facts. Actor controllers provide behavior decisions but are not cognition frameworks and do not bypass world validation.

## Actor state

Each `WorldActor` has its own:

- identity
- position
- orientation
- dimensions
- movement speed
- collision participation
- action queue
- optional controller

The existing world persistence model already preserves the actor's physical state and identity. Action queues and controllers are runtime behavior state rather than world snapshot data.

## Actor actions

The current phase uses a small validated action vocabulary:

```text
MOVE(directionX, directionY, duration)
TURN(deltaDegrees)
WAIT(duration)
```

Actions are queued on the actor. The world executes them during `World.Update` rather than allowing controllers to modify actor position directly.

Movement directions are normalized before physical movement so actor speed remains authoritative.

## Controllers

`IWorldActorController` is the non-LLM behavior boundary for Phase 3.

A controller receives a `WorldActorControllerContext` when its actor is idle. It may request world actions through the context, but the world still owns:

- speed limits
- world bounds
- item collision
- actor collision
- action execution
- simulation time

This keeps the simulation useful without introducing a model/provider dependency.

## Deterministic update ordering

`World.Update(deltaSeconds)` uses the actor list order as the deterministic ordering for Phase 3 experiments:

1. Ask idle actors with controllers for their next action.
2. Execute one current action for each actor in the same actor order.
3. Advance simulation time.

A controller therefore cannot directly advance the simulation or execute another actor's action.

## Actor collision

Colliding actors use the same body-space AABB collision rule as solid world items. A move is rejected when the proposed actor body intersects another colliding actor.

Spawning and restoration also reject overlapping colliding actors. Non-colliding actors remain physically pass-through.

Phase 3 intentionally uses a direct actor scan for collision because the actor population is expected to be small. A dedicated actor spatial index can be introduced later when measurements justify it.

## Actor perception

The Geometry Eye can optionally include actors through `WorldGeometryCamera.IncludeActors`.

For each visible actor, the sensor emits the same anonymous geometric observation contract used for items:

- anonymous entity ID
- relative X/Y
- distance
- bearing
- width/height
- rotation
- solid state

The observing actor is excluded from its own sensor results.

Actor names, semantic kinds, private state and hidden intentions are not added to the observation.

## Independent sensors

Each actor may own a separate `WorldGeometryCamera` instance and therefore have independent settings such as FOV, range and solid-state inclusion. Sensor instances remain renderer-independent and are owned by the observing execution context.

## Laboratory

`HWorld.Example` exposes a Multi-Actor Laboratory that demonstrates:

- two actors in one continuous world;
- independent non-LLM controllers;
- actor-versus-actor collision;
- separate Geometry Eye instances;
- actor-to-actor perception;
- exact serialized observations for both actors;
- approximate token estimates for both observations;
- the same authoritative world rendered alongside the two sensor views.

The laboratory is an experiment harness, not part of HWorld.Core.

## Boundary to future scheduling

Phase 3 deliberately does not implement asynchronous model requests, decision latency, cancellation, timeouts or fairness scheduling. Those belong to the documented Phase 4 time/decision-scheduling milestone.

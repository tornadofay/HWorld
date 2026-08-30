# HWorld Architecture

## Project boundaries

HWorld is split by responsibility:

```text
HWorld.Core
    renderer-independent world/simulation library

HWorld.WinForms
    reusable Windows Forms integration
    GDI+ renderer/viewer
    world designer
    WinForms file-dialog integration

HWorld.Console
    console renderer/runtime

HWorld.Example
    executable test harness
    exercises the libraries above
    contains test/sample world fixtures only
```

`HWorld.Example` must not become the owner of rendering implementations. Its purpose is to prove that the libraries work together.

## Boundary between HWorld and HAgent

HWorld owns the simulated reality.
HAgent owns model/provider/tool/agent execution concerns.

HWorld should consume HAgent through a small adapter interface rather than referencing HAgent internals throughout the simulation.

## Core world layer

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
- spatial indexing
- collision/occupancy rules

The core must not reference GDI+, WinForms, Console rendering, DirectX, Godot, Unity, or a model provider.

## Rendering boundary

A renderer reads simulation state and presents it. It must not decide simulation outcomes.

```text
             HWorld.Core
                  |
        +---------+---------+
        |                   |
 HWorld.WinForms     HWorld.Console
        |                   |
      GDI+               terminal
```

The same `World` instance/model can therefore be presented through different front ends.

### WinForms

`HWorld.WinForms` contains reusable GDI+ presentation components, including the world canvas and designer forms. These components consume `HWorld.Core` objects and remain independent of the Example application's sample scenarios.

### Console

`HWorld.Console` contains terminal rendering and interactive console runtime code. Console presentation must not leak into `HWorld.Core`.

## Example test harness

`HWorld.Example` is intentionally thin.

Its main job is to expose test entry points such as:

- Design World
- Run GDI
- Run Console

It may contain sample/test world factories, but renderer implementation belongs to the renderer project.

## Simulation clock

Simulation time is independent of wall-clock/API latency.

A decision request may be outstanding while the world continues to advance.

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

Semantic labels should not automatically be exposed through perception APIs.

## Spatial index

Spatial indexing is an optimization and query facility, not a source of truth.

The initial implementation uses a uniform grid. Later implementations may add other strategies without changing the world API.

## Camera and sensor boundary

A camera/sensor determines what an observer can perceive.

A renderer determines how that information or world state is presented visually.

These concepts must remain separable so a geometric camera can later feed an AI observation while a GDI camera can simultaneously show a human-readable view.

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

The cycle must work in real-time, accelerated mode, or headless mode.

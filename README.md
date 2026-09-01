# HWorld

HWorld is an independent, renderer-neutral 2D artificial-world simulation platform.

It is designed for experiments where humans and AI agents inhabit the same persistent world, perceive only what their own sensors expose, interact with physical objects, and eventually reason, remember, learn, socialize, and pass learned characteristics across generations through external cognition systems.

HWorld is intentionally independent from any particular AI or cognition library. It must be useful and testable without an LLM. An external cognition system may be connected later through the generic decision boundary and may be replaced by another implementation.

## Current status

HWorld has reached the first substantial pre-AI decision-scheduling foundation. The world can be authored by hand, rendered through reusable WinForms/GDI+ or console views, saved and loaded, queried through a spatial index, played by a human-controlled actor, exercised with multiple independently embodied actors, and run with asynchronous external decision providers without blocking simulation time.

The current work is the deterministic world + multi-actor + decision-scheduling laboratory foundation. Before external cognition is connected, the Example project remains the primary test laboratory for observing what the world, sensors and scheduler actually expose.

Implemented today:

- Renderer-independent world, item, actor, geometry, collision and simulation-time model
- Vector-based world shapes and GDI+ rendering
- Console rendering
- World Designer with object placement, selection and editing
- Save/load using `.hworld.json`
- Default world storage under `Worlds\\` beside the application executable in the WinForms example
- Uniform-grid spatial index with reusable query buffers
- Human player movement
- Basic collision-aware movement
- Generic object interaction and interactable item metadata
- Multiple world actors with independent physical state
- Actor-specific action queues and deterministic actor update ordering
- Non-LLM behavior/controller boundary for autonomous actors
- Actor-to-actor collision handling
- Forward-facing geometry camera with FOV and range
- Optional actor perception through the same geometry observation contract
- Independent sensor instances per observing actor
- Compact geometry-observation serialization
- Approximate observation token-cost estimation
- Human-facing Geometry Eye visualization in the GDI runtime
- Multi-Actor Laboratory showing two independently controlled actors and their separate sensor views
- Asynchronous actor decision-provider contract
- Immutable actor decision context snapshots
- Per-actor decision cadence and timeout policy
- Concurrent decision limit
- Decision correlation IDs and lifecycle events
- Cancellation and stale/late-result protection
- Simulation-thread-only decision result application
- Real-time and deterministic-checkpoint scheduling modes
- Event-driven action-completion wake-up
- Decision Scheduling Laboratory showing different provider response speeds
- Separate `HWorld.Example` test harness, `HWorld.WinForms` renderer/designer library, and `HWorld.Console` renderer

Not yet implemented:

- External cognition integration
- Agent memory integration
- Agent hands/inventory
- Knowledge/skills integration
- Generational inheritance
- Rendered-image perception
- Occlusion-aware perception
- Multi-agent communication and social behavior

## Core rule

**The simulation is the world. A renderer is only a view of the world. A camera is only a sensor. An LLM or other cognition system is only one possible decision-making mechanism.**

## Project boundaries

```text
HWorld.Core
    World / Items / Actors / Geometry
    Time / Collision / Spatial Index
    Persistence / Perception / Action contracts
    Decision scheduling boundary
    World events and authoritative state

HWorld.WinForms
    Reusable WinForms controls
    GDI+ world renderer
    Geometry Eye renderer
    World Designer
    GDI runtime viewer

HWorld.Console
    Reusable console renderer/runtime

HWorld.Example
    Test harness and experiment laboratory
    Creates sample worlds
    Opens Designer / GDI / Console
    Runs the Multi-Actor Laboratory
    Runs the Decision Scheduling Laboratory
    Exposes camera observations and compact token text

External cognition
    Optional decision/cognition implementation
    Provider/model execution and cognitive state
```

## World vs cognition

HWorld is authoritative about **what actually exists and what actually happened**.

HWorld does not decide what an agent remembers, believes, knows, or learns.

External cognition may transform authorized experience and observations into cognition and return a decision/result:

```text
HWorld observation/event
   -> external cognition
   -> decision/result
   -> HWorld validation
   -> world action/state
```

HWorld owns the meaning, validation, and side effects of world actions.

## Perception laboratory

The `HWorld.Example` project is also a research laboratory, not just a launcher.

The Geometry Eye experiments expose the actual sensor view, exact compact observation serialization, and approximate token cost. The Multi-Actor Laboratory demonstrates separate observers sharing one world. The Decision Scheduling Laboratory demonstrates that external decision latency does not stop simulation time.

Sensors must not automatically reveal semantic object names, hidden state, exact world coordinates or off-camera information unless an experiment explicitly enables them.

## Time and decision scheduling

HWorld simulation time advances through `World.Update`. External decision providers execute asynchronously through `WorldActorDecisionScheduler`.

```text
simulation thread
    -> capture immutable actor state
    -> start decision request
    -> continue world simulation
    -> receive completion/timeout/cancellation
    -> validate returned decision
    -> enqueue action
    -> world executes action
```

The scheduler assigns every request a correlation ID, limits concurrent requests, supports cancellation, rejects invalid or stale results, and records decision lifecycle timing.

A slow provider therefore consumes decision latency without freezing the physical world.

## Renderer independence

A world item or actor is a simulation object, not a UI control. GDI+, console, and future renderers observe the same authoritative state.

## AI is optional

HWorld runs without any LLM, model API key, GPU, or external cognition library.

The next integration is an external cognition adapter at the decision boundary. HAgent is one possible implementation, not a required HWorld subsystem.

## Research direction

The project is intended to support experiments such as:

- Limited fields of view
- Occlusion
- Geometry-only perception
- Image-based perception later
- Multi-agent behavior
- Different models/providers in one world
- Sparse/conditional cognition activation
- Token and information budgets
- Agent-specific memories
- Knowledge and reusable skills
- Hands and inventory
- Human/AI interaction
- Generational inheritance
- Population ecology
- Alternative renderers

## Documentation

- [Vision](docs/vision.md)
- [Plans](docs/plans.md)
- [Roadmap](docs/roadmap.md)
- [Architecture](docs/architecture/architecture.md)
- [World Model](docs/architecture/world-model.md)
- [Caching and Performance](docs/architecture/caching-and-performance.md)
- [Decision Log](docs/architecture/decision-log.md)
- [Perception](docs/design/perception.md)
- [Agents and Time](docs/design/agents-and-time.md)
- [Multi-Actor Simulation](docs/design/multi-actor.md)
- [Decision Scheduling](docs/design/decision-scheduling.md)
- [Cognitive Layers](docs/design/cognitive-layers.md)
- [Generational Inheritance](docs/experiments/generational-inheritance.md)
- [Renderers](docs/ui/renderers.md)
- [Console Renderer](docs/ui/console-renderer.md)
- [User Experience](docs/ui/user-experience.md)
- [Experiments](docs/experiments/experiments.md)

## Development rule

Do not begin by making cognition part of the simulation core.

First make the deterministic world dependable, observable, testable, persistable, and renderer-independent. Build richer perception, multi-actor behavior and decision scheduling before connecting an external cognition system. Keep project state documented whenever a milestone changes the implementation.

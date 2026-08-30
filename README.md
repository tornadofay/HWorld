# HWorld

HWorld is an independent, renderer-neutral 2D artificial-world simulation platform.

It is designed for experiments where humans and AI agents inhabit the same persistent world, perceive only what their own sensors expose, interact with physical objects, remember experiences, accumulate knowledge and skills, interact socially, and potentially pass learned characteristics across generations.

HWorld is intentionally **independent from HAgent during its early development**. The world must become useful and testable before any LLM integration exists. HAgent will be an optional external integration later.

## Current status

HWorld has now reached the first substantial pre-AI prototype. The world can be authored by hand, rendered through reusable WinForms/GDI+ or console views, saved and loaded, queried through a spatial index, played by a human-controlled actor, and inspected through a geometry-based sensor.

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
- Forward-facing geometry camera with FOV and range
- Compact geometry-observation serialization
- Approximate observation token-cost estimation
- Human-facing Geometry Eye visualization in the GDI runtime
- Separate `HWorld.Example` test harness, `HWorld.WinForms` renderer/designer library, and `HWorld.Console` renderer

Not yet implemented:

- LLM/HAgent integration
- Agent memory
- Agent hands/inventory
- Knowledge/skills
- Multi-agent decision-making
- Generational inheritance
- Rendered-image perception

## Core rule

**The simulation is the world. A renderer is only a view of the world. A camera is only a sensor. An LLM is only one possible decision-making mechanism.**

```text
                    HWorld Simulation
                           |
        +------------------+------------------+
        |                  |                  |
      World              Agents            Items
        |                  |                  |
     Physics            Bodies          Interaction
     Collision          Sensors           State
     Spatial Index      Memory*           Affordance*
     Time              Knowledge*         Inventory*
                         Skills*

                    * optional systems

                           |
                  Renderer / Viewer API
            +--------------+--------------+
            |              |              |
         Console          GDI+      Future adapters
                                      DirectX / Godot / Unity
```

## Project boundaries

The solution is intentionally split so that rendering experiments do not become part of the simulation core.

```text
HWorld.Core
    World / Items / Actors / Geometry
    Time / Collision / Spatial Index
    Persistence / Perception contracts

HWorld.WinForms
    Reusable WinForms controls
    GDI+ world renderer
    Geometry Eye renderer
    World Designer
    GDI runtime viewer

HWorld.Console
    Reusable console renderer/runtime

HWorld.Example
    Test harness only
    Creates sample worlds
    Opens Designer / GDI / Console
```

## Why start with console and GDI+

The project should run on modest hardware and require no dedicated GPU.

The first development path is:

```text
C# world core
    -> console renderer
    -> geometry camera
    -> human player
    -> WinForms/GDI+ viewer
```

The console is not a temporary debugging toy. It is a real renderer target that can grow from a character grid into a richer ANSI/Unicode terminal presentation.

GDI+ is the default graphical viewer for the first Windows implementation because it is available in WinForms and is sufficient for 2D experimentation. The simulation core must never depend on GDI+.

## Renderer independence

A world item is a simulation object, not a GDI control.

For example:

```text
WorldItem
    Id
    Position
    Rotation
    Shape
    Size
    Physical properties
    State
    Capabilities / affordances
```

The GDI renderer may draw it as shapes, the console renderer may use characters, and a future Godot/Unity renderer may use sprites or scene objects. None of those representations changes the underlying world object.

## AI is optional

HWorld must run without any LLM or API key.

An LLM becomes a participant later through an adapter. The LLM does not run every simulation tick and does not receive the complete world state.

The intended path is:

```text
World
  -> Agent sensor
  -> compact observation
  -> optional reasoning
  -> validated action
  -> World
```

Different agents can use different providers/models, different cameras, different memory configurations, or no LLM at all.

## Research direction

The project is intended to support experiments such as:

- Forward-facing 2D cameras
- Limited fields of view
- Occlusion
- Geometry-only perception
- Image-based perception later
- Agent-specific memories
- Wiki-like knowledge
- Reusable skills
- Tool use
- Hands and inventory
- Human/AI interaction
- Multi-agent societies
- Different models/providers in one world
- Sparse/conditional LLM activation
- Token and information budgets
- Generational inheritance of learned knowledge and behavior
- Decay of inherited knowledge when environmental evidence disappears
- Co-evolution of different populations

## Important distinction: generational knowledge

HWorld's proposed inheritance experiment is not conventional DNA.

An agent may learn that an object is dangerous after observing it harm another agent. Descendants may inherit some representation of that knowledge or tendency without personally witnessing the event. If the relevant object disappears from the environment for many generations, the inherited fear may weaken or disappear.

A second population can have its own inherited knowledge and behaviors. Their interaction can therefore change the environment and create new selection pressures.

These mechanisms are configurable experiments, not assumptions about biology.

## Human player

A human-controlled character should inhabit the same simulation as AI agents.

The human and AI should interact with the same world objects and obey the same physical rules. The renderer may provide user-friendly controls, but the world itself remains authoritative.

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
- [Cognitive Layers](docs/design/cognitive-layers.md)
- [Information Economy](docs/design/information-economy.md)
- [Generational Inheritance](docs/experiments/generational-inheritance.md)
- [Renderers](docs/ui/renderers.md)
- [Console Renderer](docs/ui/console-renderer.md)
- [User Experience](docs/ui/user-experience.md)
- [HAgent Integration](docs/api/hagent-integration.md)
- [HAgent Project Plan Prompt](docs/api/HAgent-project-plan-prompt.md)
- [Experiments](docs/experiments/experiments.md)

## Development rule

Do not begin with AI.

First make the deterministic world dependable, testable, persistable, and renderer-independent. Build richer perception and interaction before connecting HAgent. Keep project state documented whenever a milestone changes the implementation.

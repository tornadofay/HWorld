# HWorld

HWorld is an independent, renderer-neutral 2D artificial-world simulation platform.

It is designed for experiments where humans and AI agents inhabit the same persistent world, perceive only what their own sensors expose, interact with physical objects, and eventually reason, remember, learn, socialize, and pass learned characteristics across generations through external cognitive systems.

HWorld is intentionally **independent from HAgent during early development**. The world must be useful and testable before any LLM integration exists. HAgent is an optional external decision/cognition integration later.

## Current status

HWorld has reached the first substantial pre-AI multi-actor prototype. The world can be authored by hand, rendered through reusable WinForms/GDI+ or console views, saved and loaded, queried through a spatial index, played by a human-controlled actor, and exercised with multiple independently embodied actors and actor-specific geometry sensors.

The current work is the deterministic multi-actor/perception foundation. Before connecting HAgent, the Example project remains the primary test laboratory for observing what the world and sensors actually expose.

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
- Separate `HWorld.Example` test harness, `HWorld.WinForms` renderer/designer library, and `HWorld.Console` renderer

Not yet implemented:

- Async decision/time scheduling with different response speeds
- LLM/HAgent integration
- Agent memory implementation
- Agent hands/inventory
- Knowledge/skills implementation
- Generational inheritance
- Rendered-image perception
- Occlusion-aware perception

## Core rule

**The simulation is the world. A renderer is only a view of the world. A camera is only a sensor. An LLM is only one possible decision-making mechanism.**

```text
                    HWorld Simulation
                           |
        +------------------+------------------+
        |                  |                  |
      World              Actors            Items
        |                  |                  |
     Physics            Bodies          Interaction
     Collision          Sensors           State
     Spatial Index      Events            Affordances
     Time              Action results
                           |
                    external cognition*
                           |
             Memory / Knowledge / Skills
                           |
                         Model*

                    * optional external systems

                           |
                  Renderer / Viewer API
            +--------------+--------------+
            |              |              |
         Console          GDI+      Future adapters
                                      DirectX / Godot / Unity
```

## Project boundaries

The solution is intentionally split so rendering experiments and cognitive systems do not become part of the simulation core.

```text
HWorld.Core
    World / Items / Actors / Geometry
    Time / Collision / Spatial Index
    Persistence / Perception / Action contracts
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
    Exposes camera observations and compact token text

HAgent (external)
    Agent execution/cognition infrastructure
    Model/provider execution
    Tool-call routing
    Optional memory/knowledge/skill systems
```

## World vs cognition

HWorld must remain authoritative about **what actually exists and what actually happened**.

HWorld should expose facts such as:

```text
Object 42 exists
Object 42 damaged Actor 7
Actor 7 moved from A to B
Actor 3 observed these geometry records
```

HWorld does not decide what an agent remembers, believes, knows, or learns.

External cognition such as HAgent may transform experience into cognition:

```text
World event
   -> HAgent memory
   -> HAgent knowledge/belief
   -> HAgent skill/behavior
   -> action request
   -> HWorld validation
```

This prevents HWorld from becoming a second agent framework.

## Perception laboratory

The `HWorld.Example` project is also a research laboratory, not just a launcher.

The Geometry Eye experiment should show both:

1. what the observer's sensor sees visually;
2. the **exact compact observation text** that an eventual external agent would receive;
3. the approximate token cost of that observation.

The Multi-Actor Laboratory additionally shows how two actors can share one authoritative world while receiving different sensor observations from their own positions and headings.

The sensor must not automatically reveal semantic object names, hidden state, exact world coordinates or off-camera information unless an experiment explicitly enables them.

## Why start with console and GDI+

The project should run on modest hardware and require no dedicated GPU.

The first development path is:

```text
C# world core
    -> console renderer
    -> geometry camera
    -> human player
    -> WinForms/GDI+ viewer
    -> multi-actor laboratory
    -> time/decision scheduling
    -> optional HAgent integration
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

An external agent system becomes a participant later through an adapter. The model does not run every simulation tick and does not receive the complete world state unless an experiment explicitly chooses a privileged mode.

The intended path is:

```text
World
  -> agent sensor
  -> compact observation
  -> optional external cognition
  -> validated action
  -> World
```

Different agents can use different providers/models, different cameras, different cognitive systems, or no LLM at all.

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
- [Multi-Actor Simulation](docs/design/multi-actor.md)
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

First make the deterministic world dependable, observable, testable, persistable, and renderer-independent. Build richer perception and interaction before connecting HAgent. Keep project state documented whenever a milestone changes the implementation.

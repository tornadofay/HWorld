# HWorld Roadmap

## v0.1 — World Kernel

A headless C# simulation can create a world, advance time, move entities, perform basic collisions and save/load state.

## v0.2 — Console Laboratory

The world can be played and inspected through a console viewport with a human-controlled agent.

## v0.3 — Geometry Eye

Agents have forward-facing geometry cameras and receive only local observations.

## v0.4 — First HAgent Brain

One agent can reason through an HAgent provider, select validated actions, and continue while the simulation clock advances independently.

## v0.5 — GDI World Viewer

The same world is visible in Windows Forms/GDI+ with full-world and per-agent camera views.

## v0.6 — Memory

Agents can store and retrieve experiences without replaying their complete history.

## v0.7 — Embodied Interaction

Agents can manipulate items, use hands, carry inventory and act through validated world actions.

## v0.8 — Knowledge + Skills

Agents can consolidate experiences into reusable knowledge and skills; shared/group knowledge is supported.

## v0.9 — Multi-Agent World

Multiple independent agents can use different providers, models, cameras, memories and cognitive rates in the same continuous simulation.

## v1.0 — Human + AI World

A non-technical user can configure an AI provider with their own credentials, create/run a world, control a human character, and interact with AI agents.

## Post-1.0 research tracks

### Perception

- Image cameras
- Visual models
- Sensor fusion
- Noise and limited information

### Society

- Cooperation
- Conflict
- Reputation
- Communication
- Group norms
- Shared knowledge

### Evolution

- Generational inheritance
- Forgetting
- Cultural transmission
- Mutation of behavioral tendencies
- Population ecology

### Rendering

- High-performance native renderer
- Godot
- Unity

### Scale

- Thousands of cheap rule-driven entities
- Sparse LLM activation
- Event-driven reasoning
- Local models where available

## Roadmap rule

No milestone should make the simulation dependent on a particular renderer, model provider, or GPU.

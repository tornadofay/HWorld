# HWorld Roadmap

## Current release state — Pre-AI foundation

The project has completed the initial deterministic-world foundation and the first perception/interaction experiments. The simulation does not depend on HAgent, an LLM, a GPU, or a particular renderer.

### Completed

- **World Kernel:** world container, IDs, 2D geometry, actors, items, simulation time, deterministic updates
- **World interaction:** collision-aware actor movement, interactable objects, generic interaction results
- **Persistence:** JSON world snapshots with preserved IDs/state
- **Spatial indexing:** uniform-grid spatial index with reusable query buffers
- **Vector world art:** renderer-independent shape metadata with GDI+ rendering
- **Console Laboratory:** playable console world with human movement and camera/viewport behavior
- **WinForms/GDI+:** reusable GDI renderer and runtime viewer
- **World Designer:** place, select, move, resize, rotate, edit, and delete world items
- **Geometry Eye:** forward-facing FOV/range perception with relative geometry observations
- **Observation economy:** compact geometry serialization and approximate token estimation
- **Project separation:** Core simulation, WinForms renderer/designer, Console renderer, and Example test harness are separated

## v0.1 — World Kernel

**Status: Complete**

A headless C# simulation can create a world, advance time, move entities, perform basic collisions, query nearby entities, interact with items, and save/load state.

## v0.2 — Console Laboratory

**Status: Complete**

The world can be played and inspected through a console viewport with a human-controlled agent.

## v0.3 — Geometry Eye

**Status: Complete (initial geometry sensor)**

Agents/observers have a forward-facing geometry camera with FOV and range and receive local geometric observations. Compact serialization and token estimation are implemented. Occlusion and richer sensor models remain future work.

## v0.4 — First HAgent Brain

**Status: Planned**

One agent can reason through an HAgent provider, select validated actions, and continue while the simulation clock advances independently.

## v0.5 — GDI World Viewer

**Status: Complete (initial viewer)**

The same world is visible in Windows Forms/GDI+ with a full-world view and Geometry Eye view, plus a reusable World Designer.

## v0.6 — Memory

**Status: Planned**

Agents can store and retrieve experiences without replaying their complete history.

## v0.7 — Embodied Interaction

**Status: Planned**

Agents can manipulate items, use hands, carry inventory and act through validated world actions.

## v0.8 — Knowledge + Skills

**Status: Planned**

Agents can consolidate experiences into reusable knowledge and skills; shared/group knowledge is supported.

## v0.9 — Multi-Agent World

**Status: Next major milestone**

Multiple independent actors can inhabit one continuous simulation, observe one another through their own sensors, and use independent movement/action state. Later this becomes the base for different model/provider agents in the same world.

## v1.0 — Human + AI World

A non-technical user can configure an AI provider with their own credentials, create/run a world, control a human character, and interact with AI agents.

## Post-1.0 research tracks

### Perception

- Occlusion
- Narrow/wide/omnidirectional cameras
- Ray/lidar-like sensors
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

- Higher-performance native renderer
- DirectX/Direct2D-style backend
- Godot
- Unity

### Scale

- Thousands of cheap rule-driven entities
- Sparse LLM activation
- Event-driven reasoning
- Local models where available
- Efficient observation/token budgets

## Roadmap rule

No milestone should make the simulation dependent on a particular renderer, model provider, or GPU. Changes that alter the implemented state must update the README, this roadmap, the active implementation plan, and relevant detailed docs.

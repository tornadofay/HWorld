# HWorld Roadmap

## Current release state — Pre-AI decision-scheduling foundation

The deterministic world, first perception experiments, multi-actor foundation, and the first asynchronous decision-scheduling boundary are implemented. The simulation remains independent of HAgent, a specific LLM provider, a GPU, and a particular renderer.

### Completed

- **World Kernel:** world container, IDs, 2D geometry, actors, items, simulation time, deterministic updates
- **World interaction:** collision-aware actor movement, interactable objects, generic interaction results
- **Persistence:** JSON world snapshots with preserved IDs and actor state
- **Spatial indexing:** uniform-grid spatial index with reusable query buffers
- **Vector world art:** renderer-independent shape metadata with GDI+ rendering
- **Console Laboratory:** playable console world with human movement and camera/viewport behavior
- **WinForms/GDI+:** reusable GDI renderer and runtime viewer
- **World Designer:** place, select, move, resize, rotate, edit, and delete world items
- **Geometry Eye:** forward-facing FOV/range perception with relative geometry observations
- **Observation economy:** compact geometry serialization and approximate token estimation
- **Multi-actor foundation:** multiple actors, independent actor state, actor action queues, deterministic controller/update order, actor collision, actor perception
- **Decision scheduling foundation:** asynchronous decision providers, immutable decision snapshots, per-actor cadence, timeout/cancellation, correlation IDs, stale-result protection, concurrent-request limits, action-completion wake-up, and real-time/deterministic-checkpoint scheduling modes
- **Project separation:** Core simulation, WinForms renderer/designer, Console renderer, and Example test harness are separated

## v0.1 — World Kernel

**Status: Complete**

## v0.2 — Console Laboratory

**Status: Complete**

## v0.3 — Geometry Eye

**Status: Complete (initial geometry sensor)**

Observers have a forward-facing geometry camera with FOV and range and receive local geometric observations. Compact serialization and token estimation are implemented. Occlusion, richer projection, and alternative sensor models remain future work.

## v0.4 — First HAgent Brain

**Status: Planned**

One actor can reason through an HAgent provider, select validated actions, and continue while the simulation clock advances independently.

HWorld remains responsible for world state, observations, events, and action validation. HAgent remains an external agent execution/cognition system.

## v0.5 — GDI World Viewer

**Status: Complete (initial viewer)**

## v0.6 — Cognitive Systems Boundary

**Status: Planned / owned primarily by external agent infrastructure**

HWorld does **not** become a second memory/agent framework. HWorld provides world events, observations, action outcomes, and persistence boundaries that an external cognitive system can consume.

## v0.7 — Embodied Interaction

**Status: Planned**

Agents can manipulate items, use hands, carry inventory and act through validated world actions.

## v0.8 — Knowledge + Skills

**Status: Planned / primarily external cognition**

External cognitive systems can consolidate experiences into reusable knowledge and skills. HWorld supplies authoritative events and observable world state.

## v0.9 — Multi-Agent World

**Status: Foundation complete; autonomous-agent layer remains planned**

The deterministic multi-actor and asynchronous decision foundations are now implemented. Multiple actors can share one continuous simulation, have independent physical state, queues, controllers and sensors, and receive decisions without blocking world time. Autonomous HAgent-backed behavior, communication and social behavior remain later work.

## Phase 4 — Time and decision scheduling

**Status: Implementation complete; local build verification pending**

The generic scheduler boundary is now implemented in `HWorld.Core`. It separates simulation progression from decision latency, supports per-actor cadence and timeout policy, cancellation, lifecycle events, concurrent-request limits, stale-result protection, action-completion wake-up, and deterministic-checkpoint experiments.

`HWorld.Example` includes a Decision Scheduling Laboratory with deliberately different provider latencies so the separation can be observed without HAgent.

## Phase 5 — First HAgent Brain

**Status: Planned**

Connect `IWorldActorDecisionProvider` to HAgent through an adapter. No change to the HWorld physical authority is required.

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

## Architecture rule for cognition

HWorld owns **what exists and what happens**. External cognition such as HAgent owns **what an agent remembers, believes, knows, learns, and decides**.

## Roadmap rule

No milestone should make the simulation dependent on a particular renderer, model provider, or GPU. Changes that alter the implemented state must update the README, roadmap, active implementation plan, and relevant detailed docs.

# HWorld Roadmap

## Current release state — Pre-AI decision-scheduling foundation

The deterministic world, first perception experiments, multi-actor foundation, and asynchronous decision-scheduling boundary are implemented. HWorld remains independent of any particular cognition library, LLM provider, GPU, or renderer.

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

## v0.4 — External Cognition Integration

**Status: Planned**

Connect one external cognition implementation through the existing generic decision boundary. HAgent may be the first implementation, but it is not part of the HWorld domain model and may be replaced by another provider/library.

Requirements:

- long-lived cognition runtime associated with an actor while that actor exists;
- authorized observation/context supplied by HWorld;
- structured decision/result handling;
- HWorld-owned action definitions and validation;
- asynchronous execution without stopping simulation time;
- cancellation, timeout, correlation, and stale-result handling;
- HWorld remains fully runnable without external cognition.

## v0.5 — GDI World Viewer

**Status: Complete (initial viewer)**

## v0.6 — Cognitive Systems Boundary

**Status: Planned**

HWorld provides authoritative events, observations, action outcomes, and host-owned persistence boundaries. External cognition determines what an agent remembers, believes, knows, learns, or forgets.

HWorld must not become a second cognitive framework.

## v0.7 — Embodied Interaction

**Status: Planned**

Agents can manipulate items, use hands, carry inventory and act through validated world actions.

## v0.8 — Knowledge + Skills

**Status: Planned / primarily external cognition**

External cognitive systems can consolidate experiences into reusable knowledge and skills. HWorld supplies authoritative events and observable world state.

## v0.9 — Multi-Agent World

**Status: Foundation complete; autonomous cognition remains planned**

Multiple actors can share one continuous simulation, have independent physical state, queues, controllers and sensors, and receive decisions without blocking world time. Autonomous external cognition, communication and social behavior remain later work.

## Phase 4 — Time and decision scheduling

**Status: Implementation complete; local build verification pending**

The generic scheduler boundary separates simulation progression from decision latency and supports per-actor cadence and timeout policy, cancellation, lifecycle events, concurrent-request limits, stale-result protection, action-completion wake-up, and deterministic-checkpoint experiments.

## Phase 5 — External Cognition Integration

**Status: Planned**

Implement the first HWorld-side adapter for an external cognition library. The adapter must remain outside `HWorld.Core` and must translate external results into HWorld-owned validated actions.

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
- Sparse cognition activation
- Event-driven reasoning
- Local models where available
- Efficient observation/token budgets

## Architecture rule for cognition

HWorld owns **what exists and what happens**. External cognition owns **what an agent remembers, believes, knows, learns, and decides**.

The external cognition implementation is replaceable. HWorld must not depend on a particular library or model provider.

## Roadmap rule

No milestone should make the simulation dependent on a particular renderer, model provider, cognition library, or GPU. Changes that alter the implemented state must update the README, roadmap, active implementation plan, and relevant detailed docs.

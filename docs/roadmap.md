# HWorld Roadmap

## Current release state — External cognition integration foundation

The deterministic world, first perception experiments, multi-actor foundation, asynchronous decision-scheduling boundary, and first external cognition adapter are implemented. HWorld remains independent of any particular cognition library, LLM provider, GPU, or renderer.

HWorld has two distinct cognition integration levels:

1. **Decision/execution integration:** HWorld supplies an authorized snapshot/observation and receives an external decision that is translated into HWorld-owned validated actions.
2. **Persistent cognition integration:** a future integration can connect HWorld world events and observations to a long-lived external cognitive runtime that maintains attention, goals, intentions, plans, memory and selective deliberation.

The second level must not turn HWorld into a cognitive framework. HWorld remains authoritative for world state, simulation time, physical execution, collision, action validation, and side effects.

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
- **External cognition reference adapter:** `HWorld.HAgent` integrates HAgent outside `HWorld.Core` through the generic decision boundary and translates structured cognition results into HWorld-owned validated actions
- **Live cognition laboratory:** an actor can receive structured movement decisions from a persistent HAgent runtime instance without blocking simulation updates
- **Project separation:** Core simulation, external-cognition adapter, WinForms renderer/designer, Console renderer, and Example test harness are separated

## v0.1 — World Kernel

**Status: Complete**

## v0.2 — Console Laboratory

**Status: Complete**

## v0.3 — Geometry Eye

**Status: Complete (initial geometry sensor)**

Observers have a forward-facing geometry camera with FOV and range and receive local geometric observations. Compact serialization and token estimation are implemented. Occlusion, richer projection, and alternative sensor models remain future work.

## v0.4 — External Cognition Integration

**Status: Implemented as a reference integration; stabilization continues**

The first external cognition implementation is now connected through the generic decision boundary. HAgent is the current reference implementation, but HWorld does not depend on HAgent types in its world domain model and must remain replaceable.

Requirements now exercised by the reference integration:

- long-lived cognition runtime associated with an actor while that actor exists;
- authorized observation/context supplied by HWorld;
- structured decision/result handling;
- HWorld-owned action definitions and validation;
- asynchronous execution without stopping simulation time;
- cancellation, timeout, correlation, and stale-result handling;
- HWorld remains fully runnable without external cognition.

Remaining work is hardening and broader validation rather than defining the initial boundary.

## v0.5 — GDI World Viewer

**Status: Complete (initial viewer)**

## v0.6 — Cognitive Systems Boundary

**Status: Planned**

HWorld will provide the authoritative environment boundary for external cognitive systems:

- world events and event provenance;
- authorized observation snapshots;
- action outcomes and relevant timestamps;
- actor identity and visibility rules;
- persistence/replay hooks where required.

External cognition determines what an agent remembers, believes, knows, learns, forgets, attends to, plans, and decides.

A future persistent cognition integration should be event-driven and selective rather than invoking an LLM for every simulation tick, movement step, or routine event. Routine/reactive behavior should be able to continue without deliberation when the external cognition system supports it.

HWorld must not become a second cognitive framework.

## v0.7 — Embodied Interaction

**Status: Planned**

Agents can manipulate items, use hands, carry inventory and act through validated world actions.

The physical mechanics, reachability, collision, affordances, and action validation belong to HWorld. The choice of procedure, intention, or plan belongs to external cognition.

## v0.8 — Knowledge + Skills

**Status: Planned / primarily external cognition**

External cognitive systems can consolidate experiences into reusable knowledge and skills. HWorld supplies authoritative events, observations, outcomes, and world constraints. HWorld does not own the policy for memory, knowledge or skill formation.

## v0.9 — Multi-Agent World

**Status: Foundation complete; autonomous persistent cognition remains planned**

Multiple actors can share one continuous simulation, have independent physical state, queues, controllers and sensors, and receive decisions without blocking world time. Autonomous external cognition, communication and social behavior remain later work.

Future multi-agent cognition must allow different actors to use different external cognition implementations, execution targets, cognitive rates, sensors, memories and stores in the same simulation.

## Phase 4 — Time and decision scheduling

**Status: Implementation complete; local build verification pending**

The generic scheduler boundary separates simulation progression from decision latency and supports per-actor cadence and timeout policy, cancellation, lifecycle events, concurrent-request limits, stale-result protection, action-completion wake-up, and deterministic-checkpoint experiments.

This layer is intentionally independent from provider selection and must remain useful when the external cognition system changes execution targets, providers, models, quotas, or inference latency.

## Phase 5 — External Cognition Integration

**Status: Reference implementation available; stabilization and verification ongoing**

The first HWorld-side adapter for an external cognition library now exists outside `HWorld.Core`. The adapter translates external results into HWorld-owned validated actions while preserving the generic `IWorldActorDecisionProvider` boundary.

The HWorld-side contract remains responsible for:

- authorized observation/context;
- actor/world identity mapping;
- structured result validation and translation;
- action queue integration;
- action result feedback;
- failure, timeout and cancellation handling;
- stale-result protection;
- keeping HWorld runnable when cognition is unavailable.

HAgent is a reference implementation, not a required HWorld dependency.

Provider/model selection, capability discovery, quotas, rate limits, retries, execution admission, and provider-specific transport belong to the external cognition system. HWorld should consume the resulting generic execution behavior and decide what world-side policy to apply when cognition is delayed, unavailable, rejected, or degraded.

## Phase 6 — Persistent Cognitive Runtime Integration

**Status: Planned**

Goal: integrate HWorld with a long-lived external cognitive runtime without moving cognition into the world engine.

### HWorld responsibilities

- emit authoritative world events and observations;
- provide bounded, authorized context for cognitive activations;
- expose action outcomes and relevant simulation timestamps;
- validate and apply requested world actions;
- maintain simulation progression independently of cognition latency;
- prevent stale or superseded cognitive results from mutating world state;
- define world-side fallback/degradation policy when cognition is unavailable;
- preserve deterministic/replayable world behavior where the simulation mode requires it.

### External cognition responsibilities

- attention/salience and activation decisions;
- working cognitive state;
- goals, intentions and plans;
- selective memory/knowledge/skill retrieval;
- reactive behavior for routine cases where appropriate;
- deliberation only when justified by cognitive policy;
- provider/model selection and capability-aware execution;
- execution resilience, rate/quota/concurrency admission and long-running inference handling.

### Architectural rule

HWorld should not call an external LLM merely because a simulation tick occurred. A persistent cognitive runtime should be able to remain active across world time, ingest events, continue deterministic plans/reactive behaviors where possible, and request expensive deliberation only when attention and policy justify it.

## Phase 7 — Embodied Interaction

**Status: Planned**

Goal: turn agents into fully embodied participants.

### Deliverables

- Body model
- Hands/limbs
- Reachability
- Grab/release
- Inventory
- Object affordances
- Interaction validation
- Physical tool actions

The physical mechanics belong to HWorld. The choice of procedure or intention belongs to external cognition.

## Phase 8 — Knowledge and Skills

**Status: Planned / primarily external cognition**

Goal: distinguish remembered events from reusable understanding and procedures.

### HWorld responsibilities

- provide authoritative events and observations;
- expose visibility/ownership rules;
- expose world objects and action outcomes;
- provide provenance needed by external cognitive systems.

### External cognition responsibilities

- semantic knowledge store;
- wiki-like knowledge representation;
- knowledge retrieval;
- skill representation;
- skill invocation;
- skill versioning;
- provenance from experience to knowledge;
- optional shared/group knowledge.

## Phase 9 — Multi-Agent Society

**Status: Planned after the multi-actor foundation and decision scheduling**

Goal: allow multiple autonomous agents to inhabit one world.

### Deliverables

- Independent cameras
- Independent memory
- Independent knowledge
- Independent skills
- Independent model/provider configuration
- Agent-to-agent interaction
- Communication
- Group behavior
- Reputation and social state

Different actors may use different external cognition implementations, models, providers, execution targets, cognitive rates, sensors and cognitive stores in the same simulation.

## Phase 10 — Generational Inheritance

**Status: Planned**

Goal: explore inheritance of learned knowledge and behavioral tendencies.

### Deliverables

- Parent/child relationships
- Inherited knowledge
- Inherited skills
- Inherited behavioral tendencies
- Strength/weight of inherited traits
- Decay across generations
- Population/group inheritance policies
- Environmental relevance/forgetting
- Co-evolution experiments

HWorld models the world and lineage facts. External cognition determines how information is remembered, generalized, inherited, forgotten, or converted into behavior.

## Phase 11 — Advanced Perception

**Status: Planned**

Goal: progressively replace simple geometry perception with richer sensors.

### Deliverables

- Narrow/wide camera
- Omnidirectional sensor
- Ray/lidar-like sensor
- Rendered image camera
- Visual noise
- Sensor resolution settings
- Sensor bandwidth/token budget
- Sensor fusion
- Occlusion-aware perception

## Phase 12 — Alternative renderers

**Status: Planned**

Goal: prove renderer independence.

Possible sequence:

1. Higher-performance native/DirectX-style renderer
2. Godot integration
3. Unity integration

All must consume the same renderer-independent world/state interfaces.

## Phase 13 — Research tooling

**Status: Planned**

- Deterministic replay
- Experiment profiles
- Random seeds
- Event timeline
- Token/cost accounting
- Agent decision traces
- Observation snapshots
- Memory lineage
- Knowledge lineage
- Skill evolution history
- Population statistics
- CSV/JSON export

## Performance principle

Measure before optimizing, but design hot paths for low allocation from the beginning. Keep spatial queries reusable, keep renderers out of the simulation core, and avoid sending redundant information to external cognition systems.

External cognition may have much higher and more variable latency than simulation updates. HWorld must therefore remain responsive and correct while cognitive requests are pending, cancelled, failed, retried, throttled, or superseded.

## Architecture rule for cognition

HWorld owns **what exists and what happens**. External cognition owns **what an agent remembers, believes, knows, learns, attends to, intends, plans, and decides**.

HWorld applies and validates physical/world effects. External cognition requests actions or intentions through replaceable interfaces.

The external cognition implementation is replaceable. HWorld must not depend on a particular library, provider, model, execution target, or cognitive runtime implementation.

## Roadmap rule

No milestone should make the simulation dependent on a particular renderer, model provider, cognition library, or GPU. Changes that alter the implemented state must update the README, roadmap, active implementation plan, and relevant detailed docs.

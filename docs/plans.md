# HWorld Implementation Plans

> Development rule: HWorld is implemented independently of HAgent until the world core and perception/action boundaries are proven. No LLM is required for the current pre-AI foundation.

## Current state

The following foundation work is complete:

- World container, items, actors and stable identifiers
- 2D vector/geometry primitives and vector shape metadata
- Simulation clock and deterministic update loop
- Collision-aware actor movement
- Uniform-grid spatial index with reusable query buffers
- JSON world serialization/save/load
- Human-controlled player
- Console renderer/runtime
- Reusable WinForms/GDI+ renderer/runtime
- World Designer with placement and object editing
- Generic item interaction API
- Forward-facing geometry camera with FOV/range
- Compact geometry observation serialization
- Approximate observation token estimator
- Geometry Eye visualization
- Clean project separation between Core, WinForms, Console and Example

## Phase 0 — Foundation

**Status: Complete**

Goal: establish a stable, renderer-independent simulation core before AI integration.

### Completed deliverables

- Solution/project structure
- World container
- Entity and item identifiers
- 2D vector/geometry primitives
- Simulation clock
- Deterministic tick/update loop
- Spatial indexing abstraction and implementation
- Collision abstraction and basic collision-aware movement
- Serialization of world state
- Headless/renderer-independent execution path

### Rules

No GDI dependency in the simulation layer.
No HAgent dependency in the world/physics layer.

## Phase 1 — Console world

**Status: Complete**

Goal: prove the world works without graphics APIs.

### Completed deliverables

- Character-grid console renderer
- Viewport/camera positioning
- Human player movement
- World boundaries and obstacles
- Items
- Basic interactions
- Save/load
- Simulation progression

The console renderer remains intentionally simple. It is an independent rendering target and can later grow into richer ANSI/Unicode presentation.

## Phase 2 — Geometry perception

**Status: Initial implementation complete**

Goal: allow an observer to perceive a limited local world without semantic labels being supplied by the engine.

### Completed

- Forward-facing geometry camera
- Field of view
- Range
- Relative coordinates
- Distance
- Angular position
- Size and rotation
- Solid-state observation
- Compact serializer
- Approximate token-cost estimator
- GDI Geometry Eye visualization

### Remaining

- Proper occlusion
- More precise shape projection
- Actor/entity observations
- Alternative camera models
- Sensor noise/resolution controls

## Phase 3 — Multi-actor simulation

**Status: Next implementation phase**

Goal: establish multiple independently embodied actors before adding LLMs.

### Deliverables

- Multiple actors in one world
- Independent actor bodies
- Actor-specific movement state
- Actor-specific sensor instances
- Actor-to-actor collision handling
- Actor observation of other actors
- Basic non-LLM behavior/controller interface
- Deterministic actor update ordering
- Independent actor action queues

This phase should prove that several autonomous entities can share one world without sharing private perception state.

## Phase 4 — Time and decision scheduling

**Status: Planned**

Goal: separate simulation time from decision/response time.

### Deliverables

- Continuous simulation clock
- Per-actor decision cadence
- Action duration
- Async decision lifecycle
- Timeouts/cancellation
- Faster/slower decision agents
- Deterministic scheduling mode for experiments
- Event-driven action completion

A slow model must not freeze the world. A faster model must not receive an unfair assumption of instantaneous physical execution.

## Phase 5 — First HAgent Brain

**Status: Planned**

Goal: connect one actor to HAgent while keeping HWorld fully usable without it.

### Deliverables

- HAgent adapter
- Observation -> model context
- Structured action output
- Tool/action validation
- Async decision requests
- Per-agent reasoning cadence
- Action queue
- Action result feedback
- Failure/timeout handling

The HAgent integration belongs at the boundary between perception/action and the external decision engine. HWorld remains the authority over physical state.

## Phase 6 — Memory

**Status: Planned**

Goal: give agents persistent experience without replaying their complete history.

### Deliverables

- Working memory
- Episodic memory
- Memory store interface
- Search/retrieval
- Importance/salience
- Forgetting policies
- Memory limits
- Tool interface for read/write memory

Memory retrieval should be selective; the complete history should not be sent to the model every turn.

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

The model requests high-level actions. HWorld validates and performs them.

## Phase 8 — Knowledge and Skills

**Status: Planned**

Goal: distinguish remembered events from reusable understanding and procedures.

### Deliverables

- Semantic knowledge store
- Wiki-like knowledge representation
- Knowledge retrieval
- Skill representation
- Skill invocation
- Skill versioning
- Provenance from experience to knowledge
- Optional shared/group knowledge

## Phase 9 — Multi-Agent Society

**Status: Planned after multi-actor foundation**

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

Different agents may use different models, providers, prompt/configuration styles, cognitive rates, sensors and memories in the same simulation.

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

Inheritance is experimental and configurable. It must support full inheritance, partial inheritance, cultural transmission and no inheritance.

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

Measure before optimizing, but design hot paths for low allocation from the beginning. Keep spatial queries reusable, keep renderers out of the simulation core, and avoid sending redundant information to external models.

## Documentation rule

Whenever a milestone changes implementation state, update:

- `README.md`
- `docs/roadmap.md`
- `docs/plans.md`
- the relevant detailed design document under `docs/`

Do not mark a milestone complete until the code and documentation agree.

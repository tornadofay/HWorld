# HWorld Implementation Plans

> Development rule: HWorld is implemented independently of HAgent until the world core is proven. No LLM is required for Phases 0–2.

## Phase 0 — Foundation

Goal: establish a stable, renderer-independent simulation core before AI integration. The first executable world must work as a headless/console simulation with no HAgent dependency.

### Deliverables

- Solution/project structure
- World container
- Entity and item identifiers
- 2D vector/geometry primitives
- Simulation clock
- Deterministic tick/update loop
- Basic event system
- Spatial indexing abstraction
- Collision abstraction
- Serialization of world state
- Headless execution mode

### Rules

No GDI dependency in the simulation layer.
No HAgent dependency in the physics/world layer.

## Phase 1 — Console world

Goal: prove the world works without graphics APIs.

### Deliverables

- Character-grid renderer
- Viewport and camera positioning
- Agent movement
- Walls/obstacles
- Items
- Basic interactions
- Human-controlled agent
- Save/load
- Simulation speed controls

Console output may later evolve from simple character rendering into an ANSI/Unicode terminal renderer with color, panels and viewport scrolling.

## Phase 2 — Geometry perception

Goal: allow an agent to perceive a limited local world without semantic labels being supplied by the engine.

### Deliverables

- Forward-facing geometry camera
- Field of view
- Range
- Relative coordinates
- Distance
- Angular position
- Approximate visible shape/size
- Occlusion
- Unknown object IDs
- Observation serializer
- Observation token-cost estimator

The first observation format should be compact and machine-oriented.

## Phase 3 — One HAgent-controlled agent

Goal: connect the simulation to HAgent.

### Deliverables

- Agent brain adapter
- Observation -> model context
- Structured action output
- Tool/action validation
- Async decision requests
- Per-agent decision cadence
- Timeouts/cancellation
- Action queue
- Action result feedback

LLM latency must not pause world time.

## Phase 4 — Memory

Goal: give agents persistent experience without replaying the entire history.

### Deliverables

- Working memory
- Episodic memory
- Memory store interface
- Search/retrieval
- Importance/salience
- Forgetting policies
- Memory limits
- Tool interface for read/write memory

## Phase 5 — GDI+ viewer

Goal: create the default visual experience on Windows.

### Deliverables

- WinForms host
- GDI+ renderer
- World viewport
- Agent camera viewer
- Optional split view: full world + agent view
- Human controls
- Agent inspector
- Simulation controls
- Debug overlays

GDI+ is a renderer/viewer, not the world implementation.

## Phase 6 — Bodies, hands and inventory

Goal: turn agents into embodied participants.

### Deliverables

- Body model
- Limbs/hands
- Reachability
- Grab/release
- Inventory slots
- Object affordances
- Interaction validation
- Tool actions for physical interaction

The model requests high-level actions. The engine validates and performs them.

## Phase 7 — Knowledge and skills

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

## Phase 8 — Multi-agent society

Goal: allow multiple autonomous agents to inhabit one world.

### Deliverables

- Independent cameras
- Independent memory
- Independent knowledge
- Independent skills
- Independent model/provider configuration
- Social interaction
- Agent-to-agent observation
- Shared-world synchronization
- Different cognitive rates

## Phase 9 — Generational inheritance

Goal: explore inheritance of learned knowledge and behavior.

### Deliverables

- Parent/child relationships
- Inherited knowledge
- Inherited skills
- Inherited behavioral tendencies
- Strength/weight of inherited knowledge
- Decay across generations
- Population/group inheritance policies
- Environmental relevance/forgetting

Important: inheritance must be configurable. The experimenter must be able to test full inheritance, partial inheritance, cultural transmission, and no inheritance.

## Phase 10 — Advanced perception

Goal: progressively replace geometry perception with richer sensors.

### Deliverables

- Wide camera
- Narrow camera
- Omnidirectional sensor
- Ray/lidar-like sensor
- Rendered image camera
- Visual noise
- Sensor resolution settings
- Sensor bandwidth/token budget

## Phase 11 — Alternative renderers

Goal: prove renderer independence.

Possible sequence:

1. Direct2D/DirectX-style renderer
2. Godot integration
3. Unity integration

All must consume the same world/state interfaces.

## Phase 12 — Research tooling

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

Never optimize by prematurely moving logic into graphics code. First keep world logic correct and renderer-independent; optimize internal data structures when measurements justify it.

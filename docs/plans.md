# HWorld Implementation Plans

> Development rule: HWorld is implemented independently of HAgent until the world core and perception/action boundaries are proven. No LLM is required for the current pre-AI foundation.

## Architecture boundary

HWorld is the authoritative environment:

- world state
- physical bodies and objects
- simulation time
- collisions
- spatial queries
- observations/sensors
- world events
- action validation and world-side action results
- world persistence

External agent infrastructure such as HAgent is the cognition/decision layer:

- model/provider execution
- reasoning lifecycle
- tool routing
- optional memory systems
- optional knowledge/wiki systems
- optional skills/procedures
- decision policies

HWorld must expose the facts and interfaces required by those systems without implementing a second cognitive framework.

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

The `HWorld.Example` project is the test/experiment harness. It must progressively expose the internal capabilities as observable experiments rather than merely launch the renderers.

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

### Example laboratory deliverables

- Visual Geometry Eye showing exactly what the observer sensor sees
- Raw compact observation text panel showing the exact serializer output
- Approximate token estimate panel
- Camera controls for FOV/range/heading where practical
- Clear distinction between human full-world view and observer-limited sensor view

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
- Example laboratory showing two or more actors simultaneously

The experiment must demonstrate that each actor can have a different sensor view while sharing the same authoritative world.

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

The integration belongs at the perception/action boundary. HWorld remains authoritative over physical state and HAgent remains an external decision/cognition system.

## Phase 6 — Cognitive integration contracts

**Status: Planned**

Goal: expose clean interfaces for external cognitive systems without moving their implementation into HWorld.

### HWorld provides

- world events
- observation snapshots
- action results
- relevant timestamps
- actor identity and visibility rules
- persistence hooks where required

### HAgent/external cognition may provide

- working memory
- episodic memory
- retrieval
- forgetting
- semantic knowledge
- wiki-like knowledge
- reusable skills
- private/shared/group cognitive stores

The conceptual flow is:

```text
HWorld event/observation
        -> external cognition
        -> memory / knowledge / skill
        -> decision
        -> HWorld action validation
```

HWorld should not contain the policy deciding what an agent remembers or believes.

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

The physical mechanics belong to HWorld. The choice of procedure or intention belongs to the agent/cognition layer.

## Phase 8 — Knowledge and Skills

**Status: Planned / primarily external cognition**

Goal: distinguish remembered events from reusable understanding and procedures.

### HWorld responsibilities

- provide authoritative events and observations
- expose visibility/ownership rules
- expose world objects and action outcomes

### External cognition responsibilities

- semantic knowledge store
- wiki-like knowledge representation
- knowledge retrieval
- skill representation
- skill invocation
- skill versioning
- provenance from experience to knowledge
- optional shared/group knowledge

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

Different agents may use different models, providers, prompt/configuration styles, cognitive rates, sensors and cognitive stores in the same simulation.

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

HWorld models the world and lineage facts. An external cognition layer determines how information is remembered, generalized, inherited, forgotten, or converted into behavior.

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

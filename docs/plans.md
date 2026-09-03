# HWorld Implementation Plans

> Development rule: HWorld is implemented independently of any particular external cognition system until the world core and perception/action boundaries are proven. No LLM is required for the deterministic foundation.

## Architecture boundary

HWorld is the authoritative environment:

- world state
- physical bodies and objects
- simulation time
- collisions
- spatial queries
- observations/sensors
- world events
- action definitions and validation
- world-side action results
- world persistence

External cognition is a replaceable decision/cognition layer:

- model/provider execution
- reasoning lifecycle
- optional memory systems
- optional knowledge/wiki systems
- optional skills/procedures
- optional generic tools
- decision execution infrastructure
- optional persistent cognitive runtime

HWorld must expose the facts and interfaces required by external cognition without implementing a second cognitive framework.

The integration has two levels:

- **Execution/decision boundary:** HWorld supplies an authorized snapshot or observation and receives a decision that is translated into HWorld-owned validated actions.
- **Persistent cognition boundary:** a long-lived external cognitive runtime can ingest world events and observations, maintain its own cognitive state, and selectively request deliberation or actions.

HWorld remains authoritative for physical state and side effects at both levels.

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
- Multiple independent actors
- Actor-specific movement/action state
- Actor-specific sensor instances
- Actor-to-actor collision handling
- Actor observation of other actors
- Basic non-LLM behavior/controller interface
- Deterministic actor update ordering
- Independent actor action queues
- Multi-Actor Laboratory in `HWorld.Example`
- Asynchronous actor decision provider contract
- Immutable actor decision context snapshots
- Per-actor decision cadence and timeout policy
- Global concurrent-decision limit
- Decision correlation IDs and lifecycle events
- Cancellation and stale/late-result protection
- Simulation-thread-only action application
- Real-time and deterministic-checkpoint timeout modes
- Event-driven action-completion wake-up
- Decision Scheduling Laboratory in `HWorld.Example`
- First external cognition adapter in `HWorld.HAgent`, outside `HWorld.Core`
- Live HAgent-backed cognition laboratory using a long-lived runtime instance and structured decisions
- Clean project separation between Core, external cognition integration, WinForms, Console and Example

The `HWorld.Example` project is the test/experiment harness. It must progressively expose internal capabilities as observable experiments rather than merely launch the renderers.

## Phase 0 — Foundation

**Status: Complete**

Goal: establish a stable, renderer-independent simulation core before external cognition integration.

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
No external cognition or model-provider dependency in the world/physics layer.

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
- Alternative camera models
- Sensor noise/resolution controls

Actor observations are now covered by Phase 3 while preserving the same anonymous geometric observation contract.

## Phase 3 — Multi-actor simulation

**Status: Complete**

Goal: establish multiple independently embodied actors before external cognition.

### Completed deliverables

- Multiple actors in one world
- Independent actor bodies and physical state
- Actor-specific movement state
- Actor-specific sensor instances
- Actor-to-actor collision handling
- Actor observation of other actors through the geometry sensor
- Basic non-LLM behavior/controller interface
- Deterministic actor update ordering based on actor list order
- Independent actor action queues
- Validated world-side MOVE/TURN/WAIT action execution
- Multi-Actor Laboratory showing two independently controlled actors simultaneously
- Per-actor exact compact observation text and approximate token estimate

### Boundary decisions

Controllers are behavior inputs, not cognition systems. They may request validated actions but cannot directly mutate actor position or advance simulation time.

Actor collision uses the existing body AABB approach with a direct actor scan. The scan is intentionally simple for the current small-population laboratory; it should only be replaced after profiling demonstrates a need for an actor spatial index.

Action queues are runtime execution state and are not persisted in world snapshots. Actor physical state remains persistable through the existing world serializer.

## Phase 4 — Time and decision scheduling

**Status: Implementation complete; local build verification pending**

Goal: separate simulation time from decision/response time.

### Completed deliverables

- Continuous simulation clock remains owned by `World.Update`
- Per-actor decision cadence
- Action duration through the existing validated action queue
- Asynchronous decision provider contract
- Immutable decision context captured on the simulation thread
- Maximum concurrent decision requests
- Unique decision correlation IDs
- Decision lifecycle events with completion latency
- Timeout handling
- Cancellation handling
- Protection against stale/late decision results
- Real-time latency mode
- Deterministic-checkpoint timeout mode
- Event-driven action-completion wake-up
- Decision Scheduling Laboratory with intentionally different provider latencies

The scheduler never blocks `World.Update`. A provider can take longer than a simulation tick while the world continues advancing.

### Boundary decisions

`IWorldActorDecisionProvider` is a generic external decision boundary. It does not identify or depend on a particular cognition library.

`WorldActorDecisionScheduler` captures immutable actor state and optional sensor output before starting background work. It applies a returned decision only from the simulation thread through HWorld's own validated action APIs.

Timed-out/cancelled requests are retired. A provider that ignores cancellation can still finish, but its late result is no longer associated with the active scheduling path and cannot inject an action.

Decision cadence is measured in simulation seconds. In asynchronous mode, timeout uses wall-clock time. In deterministic-checkpoint mode, timeout uses simulation time.

### Laboratory verification target

The Decision Scheduling Laboratory should visibly demonstrate:

1. simulation time continues while decisions are in flight;
2. actors can have different provider latencies;
3. action execution occurs independently from provider latency;
4. decision lifecycle events report correlation and measured latency;
5. cancellation/timeout cannot inject stale actions;
6. the laboratory runs without any external cognition library.

## Phase 5 — External Cognition Integration

**Status: Reference implementation available; stabilization and verification ongoing**

Goal: integrate an external cognition implementation through the generic `IWorldActorDecisionProvider` boundary while keeping HWorld fully usable without it.

### Completed/implemented

- External cognition adapter outside `HWorld.Core`
- Authorized observation/context supplied to the external system
- Structured execution/result translation
- HWorld-owned structured action definition and validation
- Long-lived external cognition runtime associated with an actor in the reference integration
- Async decision requests
- Per-actor reasoning cadence
- Action queue integration
- Action result feedback through HWorld-side lifecycle
- Failure/timeout/cancellation handling
- Correlation and stale-result protection
- Removal of the external cognition implementation without changing the HWorld world model
- Live HAgent-backed laboratory demonstrating an LLM-driven world action

### Boundary decisions

The first external implementation is HAgent, but HWorld treats it as replaceable external infrastructure. HAgent types must not leak into `HWorld.Core`.

Provider/model selection, capabilities, quotas, rate limits, retries, execution admission, and provider transport are external-cognition concerns. HWorld consumes generic execution behavior and owns the world-side policy for what to do when cognition is unavailable, delayed, throttled, degraded, or failed.

The current continuous-decision laboratory is an integration experiment, not the intended persistent-agent architecture. It must not become the canonical pattern of issuing an LLM request for every movement step or simulation tick.

## Phase 6 — Persistent Cognitive Systems Boundary

**Status: Planned**

Goal: support a long-lived external cognitive runtime that can ingest world events, maintain cognitive state, and selectively deliberate without moving cognition into HWorld.

### HWorld responsibilities

- authoritative world events and event provenance;
- authorized observations and bounded context;
- simulation timestamps and causation/correlation metadata;
- action outcomes;
- action validation and physical/world-side execution;
- simulation progression independent of cognition latency;
- stale/superseded result rejection;
- deterministic and replay-aware world behavior where required;
- world-side fallback policy when cognition is unavailable.

### External cognition responsibilities

- attention and salience;
- working cognitive state;
- goals and priorities;
- intentions;
- plans and plan revision;
- selective memory/knowledge/skill retrieval;
- routine/reactive behavior where appropriate;
- selective deliberation;
- execution target/capability selection and provider resilience;
- independent cognitive scheduling and budgets.

### Integration principles

1. **Event-driven, not tick-driven.** A simulation tick must not automatically cause a model invocation.
2. **Selective deliberation.** Novelty, urgency, uncertainty, goal relevance, blocked progress, invalidated assumptions, explicit host requests, or similar cognitive policy can trigger deliberation.
3. **Routine continuation.** A cognitive system should be able to continue a valid plan or deterministic/reactive behavior without repeatedly reconstructing a full prompt.
4. **Separate clocks.** Simulation time, cognitive scheduling time, and external execution latency remain distinct.
5. **Bounded context.** HWorld provides only the authorized, relevant world information needed by the external runtime rather than forcing full-world serialization on every activation.
6. **No cognitive ownership in Core.** HWorld does not implement memory, belief, attention, goals, planning, knowledge, skills, or provider orchestration.

### Expected long-term flow

```text
HWorld
  -> authoritative events / observations
  -> external persistent cognitive runtime
       -> attention / goals / intentions / plans
       -> selective deliberation
       -> execution request
            -> external execution engine/provider
       -> requested world action
  -> HWorld validation and physical execution
  -> action outcome / world event
  -> cognitive runtime
```

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

The physical mechanics belong to HWorld. The choice of procedure or intention belongs to the external cognition layer.

## Phase 8 — Knowledge and Skills

**Status: Planned / primarily external cognition**

Goal: distinguish remembered events from reusable understanding and procedures.

### HWorld responsibilities

- provide authoritative events and observations
- expose visibility/ownership rules
- expose world objects and action outcomes
- provide provenance needed by external cognitive systems

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

**Status: Planned after the multi-actor foundation and persistent cognition boundary**

Goal: allow multiple autonomous agents to inhabit one world.

### Deliverables

- Independent cameras
- Independent memory
- Independent knowledge
- Independent skills
- Independent model/provider configuration
- Independent execution targets/cognitive budgets where supported
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

External cognition may have much higher and more variable latency than simulation updates. HWorld must remain responsive and correct while cognitive requests are pending, cancelled, failed, retried, throttled, or superseded.

Persistent cognition should also avoid rebuilding unchanged profile/provider/resource configuration on every activation. Stable state should remain owned by the external runtime, while HWorld sends world changes, observations and action outcomes that actually matter.

## Documentation rule

Whenever a milestone changes implementation state, update:

- `README.md`
- `docs/roadmap.md`
- `docs/plans.md`
- the relevant detailed design document under `docs/`

Do not mark a milestone complete until the code and documentation agree.

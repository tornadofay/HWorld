# HAgent Project Plan Update Prompt for HWorld

This is a planning request for the **separate HAgent project**. Do not implement HWorld inside HAgent and do not modify HWorld from the HAgent repository.

HWorld is being developed as an independent simulation project. It will eventually depend on HAgent for optional AI-agent execution, but the HWorld world/simulation must remain usable without HAgent and without an LLM.

## HWorld in one sentence

HWorld is a renderer-independent 2D artificial-world simulation in which human and AI-controlled agents can inhabit the same persistent world, perceive only what their own sensors expose, act through validated capabilities, remember experience, accumulate knowledge and skills, interact socially, and potentially pass learned characteristics across generations.

## HWorld architectural boundary

HWorld owns:

- World state
- Entities and world items
- 2D geometry
- Physics
- Collision
- Spatial indexing
- Simulation time
- Scheduling of world updates
- Agent bodies
- Hands and inventory
- Cameras and sensors
- Observation generation
- Rendering adapters
- Human player
- Populations and generations
- Memory/knowledge/skill storage for the simulated world
- Experimental configuration and replay

HAgent must remain a general-purpose reusable agent framework.

HAgent must not own:

- HWorld world state
- HWorld physics
- HWorld collision
- HWorld camera geometry
- HWorld rendering
- HWorld simulation time
- HWorld generational rules
- HWorld-specific object types
- HWorld-specific action names

## Why HWorld needs HAgent

When AI is enabled, an HWorld agent may use an HAgent-backed model/provider to interpret observations, retrieve relevant memory/knowledge, decide on high-level actions, and use tools exposed by its environment.

Different HWorld agents may use different providers, model versions, model settings, response speeds, and cognitive configurations simultaneously.

LLM response time must never stop or redefine simulation time.

## Requested HAgent planning work

Review the current HAgent architecture and project roadmap. Add only the minimum generic milestones that are required to make HWorld a clean external consumer.

### 1. Asynchronous agent execution

Plan for:

- Multiple agent executions concurrently
- Cancellation
- Timeouts
- Provider failures
- Independent execution latency
- External callers that must not block on model completion
- Start/pause/cancel/resume lifecycle where appropriate

An external environment/scheduler must be able to continue running while an agent is waiting for a model response.

### 2. Generic structured tools

HWorld will expose capabilities such as movement, looking, inspection, grabbing and inventory operations, but HAgent must not know these names.

HAgent should provide a generic mechanism for:

- Tool schemas
- Structured arguments
- Validation boundaries
- Structured tool results
- Tool-call sequencing
- Tool failure handling
- Cancellation/timeouts

The external environment remains responsible for validating and applying real-world/simulation state changes.

### 3. Observation/context boundary

HWorld may provide observations as compact text, structured data, images, or future multimodal data.

HAgent should support an external observation/context boundary without assuming that a complete environment state belongs in the prompt.

The caller must decide what context is supplied.

### 4. Memory boundary

HAgent should have a reusable memory integration point if the current implementation does not already provide one.

The design should allow an external system to:

- Retrieve relevant memories
- Store new memories
- Control context size
- Avoid replaying complete history
- Keep agent-specific memory isolated

HAgent should not hard-code HWorld memory semantics.

### 5. Knowledge and skills boundary

HWorld may later have a wiki-like knowledge system and reusable skills.

HAgent should be able to consume external knowledge or invoke externally defined skills through generic interfaces/tools.

Do not make HAgent dependent on a specific wiki implementation.

Do not implement HWorld's generational knowledge inheritance in HAgent.

### 6. Context and token/cost control

HWorld is explicitly designed to work with free/cheap API access, so context efficiency matters.

Plan generic HAgent support for:

- Caller-controlled context construction
- Compact structured messages
- Tool-result minimization
- Avoiding automatic unbounded history growth
- Token/usage telemetry when the provider exposes it
- Per-execution request/latency/usage statistics

Do not make automatic memory/history accumulation mandatory.

### 7. Provider/model independence

HWorld may run:

- Agent A on provider/model A
- Agent B on provider/model B
- Agent C on a different model version
- Agent D without an LLM

HAgent must preserve clean per-agent/provider configuration and avoid global assumptions that one model or provider serves the whole simulation.

### 8. Agent lifecycle and external scheduling

HWorld controls simulation time.

HAgent must not assume a turn-based game loop.

The generic execution API should support an external scheduler that can:

- trigger reasoning
- receive results asynchronously
- cancel stale reasoning
- ignore late results when a world state has changed
- associate a decision with an observation/context version

### 9. Telemetry

Where supported, expose useful generic execution telemetry:

- start/end time
- duration
- provider/model
- request count
- tool calls
- failures
- token/usage information
- cancellation/timeout state

This is required so HWorld can measure the information and cost behavior of different agents and cameras.

## Important design constraint

HWorld must be able to run completely without HAgent.

The following progression is intentional:

1. Pure world simulation
2. Console renderer
3. Geometry sensors
4. Human player
5. Optional HAgent integration
6. Memory/knowledge/skills
7. Multiple agents
8. Richer cameras

Therefore HAgent integration must be an adapter around the world, not the foundation of the world.

## Do not implement now

Do not:

- Add an HWorld project to the HAgent solution
- Add HWorld references to HAgent
- Add WinForms/GDI+/Godot/Unity dependencies to HAgent core
- Add world physics to HAgent
- Add camera/rendering logic to HAgent
- Add simulation scheduling/time to HAgent
- Hard-code HWorld objects or actions into HAgent
- Require an LLM for generic HAgent execution
- Add a full wiki implementation without a clear generic use case

## Deliverable

Update the HAgent project's planning documents and roadmap with small, independently testable milestones.

For every milestone document:

- Problem solved
- Why it is generic
- Existing component that should own it
- Dependencies
- API/lifecycle implications
- What explicitly remains outside HAgent
- How it can be tested without HWorld

Prefer incremental changes over a redesign. Preserve stable HAgent behavior unless a concrete architectural gap requires change.

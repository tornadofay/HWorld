# Caching and Performance

HWorld should make use of the existing caching ideas available in the HAgent ecosystem where they are generic and safe to reuse, but HWorld must not become dependent on an agent-specific cache implementation.

## What should be cached

Potentially expensive, repeatable computations include:

- spatial query candidates
- camera candidate sets
- static geometry projections
- repeated shape calculations
- compact observation serialization
- token/cost estimates
- stable knowledge retrieval results
- renderer resources

## What must not be incorrectly cached

Do not cache results that depend on mutable state without a validity policy.

Examples:

- collision results after an entity moved
- visibility after an obstacle moved
- inventory after an action
- agent state after world update

## Cache model

Prefer explicit keys and invalidation/versioning:

```text
WorldVersion
GeometryVersion
AgentStateVersion
CameraVersion
ObservationVersion
```

A cached result should identify the state/version from which it was produced.

## Token-efficiency cache

If an agent repeatedly receives nearly identical observations, HWorld may avoid rebuilding identical context and may use a delta observation where the provider/agent policy allows it.

Example:

```text
Previous:
O17=L12,D34,S
O22=R4,D12,S

Delta:
O22=D8
+O31=R20,D50,M
```

This should be an optional experiment, not a hidden protocol the model is expected to infer.

## Population scale

The goal is not to make every operation maximally fast before it is needed. Instead, keep the architecture compatible with:

- simple arrays/lists for small worlds
- spatial grids for larger worlds
- optional quadtree/spatial partitioning later
- event-driven cognition
- sparse LLM activation

Measure before replacing simple structures with more complex ones.

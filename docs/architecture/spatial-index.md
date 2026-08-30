# Spatial Index

HWorld uses a lightweight uniform-grid spatial index as a broad-phase accelerator.

## Responsibilities

The index maps world items to grid cells and supports:

- Point candidate lookup
- Area candidate lookup
- Add/remove/update
- Full rebuild

The index is not authoritative world state. `World` remains the authority; the index is an acceleration structure that can be rebuilt at any time.

## Why a uniform grid first

HWorld is a 2D world and its workloads are expected to contain many nearby entities. A uniform grid is simple, predictable, inexpensive to rebuild, and easy to debug. It also avoids committing the simulation to a more complicated tree structure before measurements justify it.

## Performance rule

Hot paths should prefer caller-owned result buffers. Avoid allocating result collections every simulation tick.

The current public API includes destination-buffer query methods for this reason.

## Future options

The abstraction should leave room for:

- Tunable cell sizes
- Dynamic/adaptive grids
- Quadtree broad phase
- Separate static and dynamic indexes
- Entity/actor indexes
- Parallel spatial queries

Those should be introduced only after profiling demonstrates a need.

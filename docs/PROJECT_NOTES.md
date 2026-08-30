# Initial Project Notes

## Working name

HWorld

## Dependency direction

```text
HWorld -> HAgent
```

Not:

```text
HAgent -> HWorld
```

HAgent should remain a reusable library.

## First environment

- C#/.NET
- Windows
- WinForms/GDI+ viewer
- Console renderer
- no GPU requirement

## First experiment

A single agent in a 2D continuous world with:

- forward-facing geometry camera
- unnamed objects
- movement
- collision
- one or more tools/actions
- compact observations
- optional HAgent-backed LLM reasoning
- no image model

## Later experiments

- memory
- skills/wiki knowledge
- object discovery and naming
- hands/inventory
- multiple agents
- different models/providers
- human player
- generational transmission and decay
- co-evolution
- image cameras
- alternative renderers

## Existing cache capability

Reuse/adapt the user's existing cache system where it cleanly fits generic simulation/perception/context workloads. Do not copy application-specific assumptions into HWorld.

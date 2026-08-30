# HWorld.Example

`HWorld.Example` is the test harness for the HWorld libraries.

It is intentionally small. It does not own the reusable GDI renderer, the console renderer, or the world designer.

## Test entry points

- **Design World** — opens `HWorld.WinForms.WorldDesignerForm`.
- **Run GDI** — opens `HWorld.WinForms.GdiWorldForm` using the GDI+ renderer.
- **Run Console** — runs `HWorld.Console.ConsoleWorldRunner`.
- **Multi-Actor Lab** — runs a two-actor deterministic simulation with independent controllers and Geometry Eye sensors.
- **Decision Lab** — runs two asynchronous decision providers with deliberately different response latencies while the shared simulation continues.

All paths exercise the same renderer-independent `HWorld.Core` concepts while using different presentation layers.

## Multi-Actor Laboratory

The laboratory demonstrates:

- two independently embodied actors sharing one authoritative world;
- separate action queues and non-LLM controllers;
- actor-versus-actor collision;
- separate sensor instances with actor-specific FOV/range;
- actor-to-actor perception through the anonymous geometry observation contract;
- the exact `WorldGeometryObservationSerializer` output for each observer.

## Decision Scheduling Laboratory

The Decision Lab demonstrates the Phase 4 scheduling boundary without HAgent or any model provider.

- one provider returns after 100 ms;
- another provider returns after 900 ms;
- both actors continue to inhabit the same continuous world;
- the world keeps advancing at 30 Hz while decisions are in flight;
- lifecycle events expose correlation IDs, outcome and latency;
- cancellation and timeout are routed through the scheduler rather than the world loop.

The controller and laboratory rendering remain example-only. Reusable simulation contracts stay in `HWorld.Core`, and reusable visualization stays in the renderer projects.

## Project responsibility

`HWorld.Example` may contain sample/test world factories and experiment scenarios.

It must not accumulate reusable rendering, designer, or simulation infrastructure that belongs in the library projects.

# HWorld.Example

`HWorld.Example` is the test harness for the HWorld libraries.

It is intentionally small. It does not own the reusable GDI renderer, the console renderer, or the world designer.

## Test entry points

- **Design World** — opens `HWorld.WinForms.WorldDesignerForm`.
- **Run GDI** — opens `HWorld.WinForms.GdiWorldForm` using the GDI+ renderer.
- **Run Console** — runs `HWorld.Console.ConsoleWorldRunner`.
- **Multi-Actor Lab** — runs a two-actor deterministic simulation with independent controllers and Geometry Eye sensors.

All paths exercise the same renderer-independent `HWorld.Core` concepts while using different presentation layers. The Multi-Actor Lab additionally exposes exact per-actor observation text and approximate token estimates.

## Multi-Actor Laboratory

The laboratory demonstrates:

- two independently embodied actors sharing one authoritative world;
- separate action queues and non-LLM controllers;
- actor-versus-actor collision;
- separate sensor instances with actor-specific FOV/range;
- actor-to-actor perception through the anonymous geometry observation contract;
- the exact `WorldGeometryObservationSerializer` output for each observer.

The controller and overview rendering remain example-only. Reusable simulation contracts stay in `HWorld.Core`, and reusable visualization stays in the renderer projects.

## Project responsibility

`HWorld.Example` may contain sample/test world factories and experiment scenarios.

It must not accumulate reusable rendering, designer, or simulation infrastructure that belongs in the library projects.

# Starting Project Structure

The initial repository is intentionally independent from HAgent.

```text
HWorld.sln
  src/
    HWorld.Core/
      World/
      Geometry/
    HWorld.Console/
    HWorld.WinForms/
```

## HWorld.Core

Owns the simulation model and must not reference GDI+, WinForms, Console APIs, Godot, Unity, DirectX, or HAgent.

The initial proof-of-life contains only a small `World`, `WorldItem`, and geometry representation. These are scaffolding, not the final world model.

## HWorld.Console

A renderer/host for the same core world. The first implementation uses a simple character grid. It is intended to grow into a controllable terminal/console renderer.

## HWorld.WinForms

The initial graphical renderer using GDI+. It is deliberately thin: it reads world state and paints it. It must not become the owner of the simulation rules.

## Next implementation order

1. Expand WorldItem and entity identity.
2. Add world update and deterministic simulation stepping.
3. Add boundaries, movement, and collision.
4. Add spatial-query abstraction and a replaceable spatial index.
5. Add world save/load.
6. Expand the console renderer into an interactive viewport.
7. Add human-controlled agent/body.
8. Add renderer-independent camera geometry.
9. Add the GDI+ world viewer and agent camera viewer.
10. Only then begin optional HAgent integration.

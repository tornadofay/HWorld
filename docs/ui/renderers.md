# Renderer Architecture

## GDI+ is the default, not the foundation

GDI+ should be the first Windows renderer because it is available, familiar, lightweight and suitable for the current environment.

The world itself must not know about `Graphics`, `Control`, `PaintEventArgs`, `Bitmap` or WinForms controls.

## Renderer contract

Conceptually:

```csharp
public interface IWorldRenderer
{
    void Render(WorldSnapshot snapshot, RenderContext context);
}
```

The exact API can evolve.

## Camera is separate from rendering

A camera/sensor determines what an observer can perceive. A renderer decides how that information is displayed to a human.

The first implemented sensor is `WorldGeometryCamera` in `HWorld.Core`. It is deterministic and reports relative geometry, range, bearing and optional solidity without semantic names.

`GeometryCameraView` in `HWorld.WinForms` is only a visualization of that sensor result. It does not change what the sensor reports.

## What a renderer can display

- world entities
- items
- agent bodies
- collision shapes
- camera FOV
- current observation
- paths
- debug information
- memory/knowledge overlays for the human observer

These overlays are for observation/debugging. They do not automatically become agent-visible information.

## GDI+ implementation

The GDI+ renderer can draw:

- the full world
- a geometry-camera viewport
- split full-world + agent-camera view
- multiple agent camera windows
- human player's view

The same projection and geometry code can be reused; GDI+ only converts world geometry into pixels.

## Current playground

The GDI runtime provides a `Geometry Eye` mode. It follows the player as observer and refreshes the geometry observation while simulation time continues normally. The full-world view and sensor view use the same `HWorld.Core.World` instance.

## Future renderers

The renderer boundary should allow:

- Direct2D/DirectX
- Godot
- Unity
- web/remote client

Each front end should visualize the same simulation state rather than reimplementing the world.

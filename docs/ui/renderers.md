# Renderer Architecture

## GDI+ is the default, not the foundation

GDI+ should be the first Windows renderer because it is available, familiar, lightweight and suitable for the user's current environment.

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
- a camera viewport
- split full-world + agent-camera view
- multiple agent camera windows
- human player's view

The same projection and geometry code can be reused; GDI+ only converts world geometry into pixels.

## Future renderers

The renderer boundary should allow:

- Direct2D/DirectX
- Godot
- Unity
- web/remote client

Each front end should visualize the same simulation state rather than reimplementing the world.

# HWorld.Example

`HWorld.Example` is the test harness for the HWorld libraries.

It is intentionally small. It does not own the GDI renderer, the console renderer, or the world designer.

## Test entry points

- **Design World** — opens `HWorld.WinForms.WorldDesignerForm`.
- **Run GDI** — opens `HWorld.WinForms.GdiWorldForm` using the GDI+ renderer.
- **Run Console** — runs `HWorld.Console.ConsoleWorldRunner`.

All three paths exercise the same renderer-independent `HWorld.Core` concepts while using different presentation layers.

## Project responsibility

`HWorld.Example` may contain sample/test world factories and test scenarios.

It must not accumulate reusable rendering, designer, or simulation infrastructure that belongs in the library projects.

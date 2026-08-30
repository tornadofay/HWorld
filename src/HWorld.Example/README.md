# HWorld Example

A small WinForms application used to exercise the HWorld simulation core without requiring HAgent or an LLM.

The example provides a professional desktop shell around the world: simulation controls, a GDI+ world viewport, camera/debug information, and status telemetry.

The example intentionally treats the world as authoritative and the renderer as a view. It is the first practical test bed for the renderer-independent simulation core.

## Current scope

- Create and run a deterministic 2D world.
- Render world items with GDI+.
- Pan and zoom the world view.
- Pause/resume simulation.
- Step the simulation manually.
- Reset the demo world.
- Display simulation time and item count.
- Keep the project independent from HAgent.

LLM integration, cameras, memory, tools, and multi-agent behavior are deliberately not required by this example yet.

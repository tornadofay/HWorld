# Project Boundary

## HWorld is independent from HAgent during core development

HWorld must be buildable, runnable and testable without the HAgent repository.

The HWorld core may define interfaces/adapters that can later consume HAgent, but the core must not require HAgent assemblies.

## Suggested dependency direction

```text
HWorld.Core
    ^
    |
HWorld.Console
HWorld.WinForms
HWorld.HAgentAdapter (later, optional)
HWorld.[future renderer adapters]
```

The renderer projects depend on the world core. They do not own world state.

The optional HAgent adapter depends on HWorld contracts and HAgent contracts; neither core project should depend on the other.

## Initial runtime target

The first implementation should favor a .NET/C# target compatible with the existing Windows development environment and future HAgent integration. Keep framework-specific code at the edges so the simulation core is portable.

A final target-framework decision should be made when the first project/solution is created, based on the HAgent compatibility requirements and renderer targets. Do not add renderer-specific dependencies to the simulation core.

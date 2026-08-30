# User Experience

## Target user

A person should be able to use HWorld without understanding AI APIs, vector math, rendering architecture or programming.

## Basic workflow

```text
Create/Open World
      -> choose world template
      -> choose graphics: Console or GDI+
      -> add human player or AI agents
      -> choose model/provider
      -> enter own API key
      -> choose agent profile
      -> Run
```

## Advanced workflow

Power users can open an experiment configuration and adjust:

- time scale
- sensor type
- FOV/range
- agent decision cadence
- memory capacity
- knowledge policies
- skill policies
- inheritance
- population rules
- renderer
- provider/model

## Safety of credentials

API keys should be stored/configured outside saved world state by default. A world file should not silently embed a provider secret.

## Human and AI parity

The human player should experience the world through a selectable camera and interact with objects according to the same physical world rules available to AI bodies.

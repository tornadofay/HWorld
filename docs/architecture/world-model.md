# HWorld World Model

## World coordinates

Use a continuous 2D coordinate space internally even when the console renderer uses a character grid.

```text
X ->
+
|
V Y
```

Renderers convert world coordinates into their own pixels/characters.

## Items

Items are world entities that can be perceived, manipulated, carried, consumed, opened, combined or otherwise acted upon.

An item may expose capabilities without revealing a semantic name.

Example internal state:

```text
Id: 42
Shape: Circle
Radius: 8
Mass: 0.4
Movable: true
Grabbable: true
Visible: true
```

The agent may only receive a compact observation such as:

```text
obj42: R31, A-12, S8, MOVEABLE, GRABBABLE
```

The meaning of that representation belongs to the configured perception contract.

## Agents

An agent is an entity with an embodied state and a decision system.

Minimum body state:

- position
- orientation
- movement capabilities
- perception sensors
- available actions

Later:

- hands/limbs
- inventory
- tools
- energy
- health
- communication channels

## Human player

The human player should use the same world entities and interaction rules as AI agents wherever practical.

The human may have direct input controls while AI agents use decisions/actions, but both operate on the same authoritative world state.

## World persistence

The world must be serializable independently of the renderer and AI provider.

A saved world should not contain provider-specific transient execution state unless explicitly included as experiment metadata.

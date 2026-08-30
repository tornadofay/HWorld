# Perception and Camera Design

## Principle

A camera is a sensor model, not merely a painting control.

The first camera is geometry-based because it is deterministic, cheap and does not require a vision model.

## Forward-facing geometry camera

`HWorld.Core.Geometry.WorldGeometryCamera` provides the first implementation.

Inputs:

- observer position
- observer rotation
- field of view
- maximum range

Processing:

```text
world position
 -> relative vector
 -> range test
 -> bearing/FOV test
 -> compact geometry observation
```

The implementation uses the world spatial index for candidate selection and reuses an internal candidate buffer between calls. The caller supplies the observation list, avoiding a mandatory result-list allocation inside the camera.

The current observation intentionally contains geometric facts only:

- entity ID
- relative X/Y
- distance
- bearing
- width/height
- rotation
- optional solid state

It does not expose the item's semantic name or application-specific kind.

This is deliberately not yet an image camera and does not perform occlusion or pixel projection.

## Camera variants

### Forward camera

A limited field-of-view cone in front of the agent. **Implemented first as `WorldGeometryCamera`.**

### Wide camera

A larger FOV that sacrifices some information efficiency.

### Omnidirectional camera

360-degree awareness; useful as a control experiment but can generate more observations.

### Ray sensor

Returns distances/encounters rather than rich visual semantics.

### Top-down camera

Useful for debugging and for studying how much a privileged perspective changes behavior.

### Rendered camera

Produces pixels through GDI+ initially, with future Direct2D/DirectX/Godot/Unity back ends.

## Observation levels

### Level 0 — geometry

No semantic names. This is the first implemented perception level.

### Level 1 — compact descriptors

Objects include rough categories/shape descriptions only if the experiment enables them.

### Level 2 — rendered image

Pixels are passed to a vision-capable model.

### Level 3 — multimodal sensor fusion

Image + geometry + touch/proximity/etc.

## Information budget

Each observation should have a measurable information/token estimate.

The engine should be able to compare:

- objects observed
- fields included
- estimated serialized size
- estimated token count
- model response size
- calls per simulated minute

## Perception should not leak world truth

Never automatically include hidden fields such as:

- exact object class
- exact world coordinates
- hidden health
- hidden intentions
- off-camera objects
- internal object names

unless the selected experiment explicitly permits them.

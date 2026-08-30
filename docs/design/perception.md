# Perception and Camera Design

## Principle

A camera is a sensor model, not merely a painting control.

The first camera should be geometry-based because it is deterministic, cheap and does not require a vision model.

## Forward-facing geometry camera

Inputs:

- observer position
- observer rotation
- field of view
- maximum range
- sensor resolution/precision
- occlusion policy

Processing:

```text
world position
 -> relative vector
 -> rotate into camera coordinates
 -> range test
 -> FOV test
 -> occlusion test
 -> project to camera space
 -> observation record
```

This supports the eventual rendered forward-facing camera without requiring actual image generation.

## Camera variants

### Forward camera

A limited field-of-view cone in front of the agent.

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

No semantic names.

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

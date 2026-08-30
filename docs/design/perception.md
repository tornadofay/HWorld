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

The current implementation provides the deterministic position/range/FOV portion. Occlusion and richer projection remain later work.

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

The current compact serializer uses only an anonymous entity ID plus relative geometry, distance, bearing, size, rotation and solid state.

### Level 1 — compact descriptors

Objects include rough categories/shape descriptions only if the experiment enables them.

### Level 2 — rendered image

Pixels are passed to a vision-capable model.

### Level 3 — multimodal sensor fusion

Image + geometry + touch/proximity/etc.

## Compact observation serialization

`WorldGeometryObservationSerializer` produces a deterministic machine-oriented text form intended to minimize context overhead.

Example structure:

```text
n=2;i=<id>,x=...,y=...,d=...,b=...,w=...,h=...,r=...,s=0;...
```

It intentionally does not serialize semantic names or application-defined kinds.

## Information budget

Each observation should have a measurable information/token estimate.

The engine should be able to compare:

- objects observed
- fields included
- serialized character count
- estimated token count
- model response size
- calls per simulated minute

`WorldObservationTokenEstimator` provides a cheap provider-neutral estimate from serialized character count. It is a planning/measurement aid, not an exact provider tokenizer or billing calculation.

## Perception should not leak world truth

Never automatically include hidden fields such as:

- exact object class
- exact world coordinates
- hidden health
- hidden intentions
- off-camera objects
- internal object names

unless the selected experiment explicitly permits them.

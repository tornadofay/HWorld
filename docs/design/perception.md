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
world state
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

## Observation laboratory

The `HWorld.Example` project must make perception directly inspectable.

For an observer, the laboratory should show side-by-side:

```text
Human full-world view
        |
        +---- Geometry Eye visual view
        |
        +---- exact compact observation text
        |
        +---- approximate token estimate
```

The displayed observation text must be the exact output of the core serializer used by the experiment. The UI must not invent a second approximation.

The laboratory should make it possible to change FOV, range, observer heading, object placement, and sensor options and immediately compare:

- what the human can see;
- what the sensor reports;
- what text an external agent would receive;
- how much information/token budget that representation consumes.

The Phase 3 Multi-Actor Laboratory extends this experiment to two independent observers so their perception can differ even inside the same world.

## Compact observation serialization

`WorldGeometryObservationSerializer` produces a deterministic machine-oriented text form intended to minimize context overhead.

Example structure:

```text
n=2;i=<id>,x=...,y=...,d=...,b=...,w=...,h=...,r=...,s=0;...
```

It intentionally does not serialize semantic names or application-defined kinds.

## Information budget

Each observation should have a measurable information/token estimate.

The engine and experiment tooling should be able to compare:

- entities observed
- fields included
- serialized character count
- estimated token count
- model response size
- calls per simulated minute
- information retained versus discarded

`WorldObservationTokenEstimator` provides a cheap provider-neutral estimate from serialized character count. It is a planning/measurement aid, not an exact provider tokenizer or billing calculation.

## Perception should not leak world truth

Never automatically include hidden fields such as:

- exact semantic object class
- exact world coordinates
- hidden health
- hidden intentions
- off-camera objects
- internal object names

unless the selected experiment explicitly permits them.

## Actor perception

Actor observations are now implemented as another geometry entity in the Level 0 contract.

A `WorldGeometryCamera` can enable or disable actor reporting through `IncludeActors`. When enabled, it:

- excludes the observing actor itself;
- applies the same range and FOV filtering as item observations;
- reports the other actor's anonymous ID, relative geometry, distance, bearing, size and rotation;
- does not expose the actor's name, controller, queue, private state or intention.

Actor observations do not receive a special semantic tag in the compact format. This preserves the deliberately anonymous Level 0 observation contract and prevents the sensor from becoming a source of hidden world knowledge.

## Independent actor sensors

Sensor instances are independent objects. Each observer can therefore use different FOV, range and solid-state settings while reading the same authoritative world.

The sensor implementation reuses local candidate buffers and is intended to be owned by one execution context; a camera instance is not thread-safe.

## HWorld/HAgent boundary

HWorld owns:

- sensor geometry
- world visibility rules
- observation generation
- world events
- authoritative physical state

HAgent or another external cognitive system may own:

- interpretation of observations
- memory of observations/events
- retrieval
- knowledge formation
- skills
- model reasoning

A renderer may visualize the observation, but it must never silently expand the information available to the agent.

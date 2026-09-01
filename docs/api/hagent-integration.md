# External Cognition Integration

## Purpose

HWorld is an independent simulation environment. External cognition is an optional integration used to provide agent decision-making. HAgent is one possible implementation; another cognition library or custom decision system may be substituted without changing HWorld's world model.

## Ownership boundary

HWorld owns:

- world state and entities;
- physical state and simulation rules;
- sensors and observations;
- simulation time and world scheduling;
- action definitions and validation;
- world-side effects;
- rendering;
- world persistence and experiment state.

External cognition owns its own:

- model/provider execution;
- reasoning/execution lifecycle;
- optional memory, knowledge, and skills;
- optional structured tools;
- provider/model selection;
- execution telemetry.

Neither side should absorb the other's domain responsibilities.

## Integration flow

```text
HWorld observation/event
        -> external cognition adapter
        -> runtime cognition instance
        -> model/reasoning
        -> provider-neutral result
        -> HWorld validation
        -> world action/state
```

The adapter belongs on the HWorld side. HWorld must not depend on the internals of a particular cognition library.

## Runtime identity

A long-lived external cognition runtime may be associated with an HWorld actor for the actor's lifetime. HWorld owns that association and its lifecycle.

The external cognition system must remain unaware of the HWorld meaning of the runtime identity.

Multiple external runtime instances may originate from one reusable configuration while remaining independently addressable.

## Observation contract

HWorld decides what an observer is allowed to perceive and supplies an explicit observation/context snapshot to external cognition.

The observation may be represented as compact text, structured data, images, or future multimodal content. External cognition must not assume that it represents complete environment state.

The same observation must not expose more information merely because a renderer can see it.

## Decision/result contract

External cognition returns a decision/result. HWorld defines what that result means in its own environment.

Do not allow external cognition to mutate authoritative HWorld state directly.

```text
cognition result
      -> HWorld validation
      -> HWorld action/state change
```

World actions, action schemas, physical constraints, and side effects remain HWorld responsibilities.

## Async execution

External reasoning may take substantially longer than one simulation update. HWorld simulation time must continue while the external request is in flight.

The integration must preserve:

- cancellation;
- timeout handling;
- request correlation;
- stale/late-result rejection;
- simulation-thread-only world mutation.

## Tools

HWorld may expose environment capabilities through a generic tool mechanism supplied by an external cognition library.

Tool definitions and execution infrastructure remain generic. The meaning and authoritative application of an environment operation remain HWorld responsibilities.

## Memory and knowledge

HWorld provides authoritative experiences, observations, events, and outcomes.

An external cognition system may transform those inputs into memory, knowledge, skills, or other cognitive state according to its own policies.

HWorld must not implement a second cognitive framework merely to support an external cognition integration.

## Replaceability rule

HWorld must be usable without any LLM or external cognition library.

The decision boundary therefore targets a generic external provider/adapter contract rather than a specific library.

HAgent may be the first implementation of that boundary, but removing HAgent must not require redesigning the world model, sensors, simulation loop, action system, or persistence.

## Current pre-integration state

The asynchronous decision scheduler is intentionally implemented without any external cognition dependency. Synthetic decision providers remain the validation mechanism for simulation timing, concurrency, timeout, cancellation, correlation, and stale-result behavior.

The next integration phase may add the first external cognition adapter without changing the world authority model.

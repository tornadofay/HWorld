# HAgent Integration

## Purpose

HAgent provides the agent execution infrastructure; HWorld provides the environment in which an agent acts.

## HWorld-to-HAgent contract

HWorld should provide:

- agent identity
- current observation
- available actions
- world events
- tool/action results
- cancellation/expiration information

HAgent should provide:

- model/provider execution
- tool-call routing
- structured action output
- execution lifecycle
- optional memory integrations
- usage information when available

## Action contract

Do not allow an LLM to mutate HWorld state directly.

The model requests an action; HWorld validates it.

```text
LLM -> action request -> HWorld validator -> world state
```

## Observation contract

Observation should be an explicit object that can be serialized into:

- compact text
- verbose text
- structured JSON
- future multimodal content

The same observation should not expose more information merely because a renderer knows it.

## Async execution

Each agent may have one or more outstanding reasoning requests depending on policy. HWorld simulation time continues while they run.

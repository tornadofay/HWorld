# Information Economy

HWorld treats information as a constrained resource.

The world may contain vastly more information than any one agent can perceive or remember.

## Three budgets

### Perception budget

How much the sensor exposes.

### Context budget

How much of the available information is sent to the reasoning model.

### Memory budget

How much the agent retains and how long it remains useful.

## Compression pipeline

```text
World truth
  -> sensor
  -> visible candidates
  -> salience filter
  -> compact observation
  -> relevant memory/knowledge retrieval
  -> context compressor
  -> model
```

## Experimental metrics

Record at least:

- observations per simulated second
- LLM requests per simulated second
- prompt tokens when available
- output tokens when available
- estimated local token count for generated context
- memory records retrieved
- knowledge records retrieved
- tool calls
- average response latency
- action success/failure

## Why this matters

A simulation with one smart agent can be useful. A simulation with thousands of entities requires information discipline.

The system should prefer:

```text
small observations + good retrieval + event-driven thought
```

over:

```text
everything + every tick + every agent
```

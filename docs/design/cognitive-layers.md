# Memory, Knowledge, Skills and Tools

## Do not call everything memory

HWorld should distinguish at least four cognitive stores.

### Working memory

Small current context required for immediate reasoning.

### Episodic memory

Events the agent experienced.

Example:

```text
I saw another agent die near object_42.
```

### Semantic knowledge

Generalized beliefs learned from experiences.

Example:

```text
object_42 is dangerous.
```

### Skills

Reusable procedures.

Example:

```text
When an object behaves like object_42, keep distance and warn group members.
```

## Tool interaction

Memory and world capabilities should be exposed through the same general tool-call architecture used by HAgent.

```text
LLM
 -> tool request
 -> HAgent execution
 -> compact result
 -> LLM continues
```

Examples:

```text
memory.search(query)
memory.store(entry)
knowledge.search(query)
skill.invoke(name, arguments)
world.inspect(objectId)
inventory.list()
body.grab(objectId)
```

## Context budgeting

Do not send all memory every turn.

Use retrieval:

```text
large stores
 -> relevance filter
 -> top useful records
 -> compact context
```

## Compression

HWorld should support compact observation and memory formats so experiments can compare verbose natural language against compressed symbolic representations.

Example:

```text
O17=L12,D34,M
O22=R4,D12,S
O31=R28,D60,U
```

The experiment configuration should document what symbols mean rather than relying on hidden conventions.

## Knowledge/wiki layer

A Wiki-like persistent knowledge layer is useful, but it should sit above raw memory rather than replace it.

```text
Experience
   -> consolidation
   -> knowledge
   -> skill proposal
```

Knowledge entries should retain provenance back to experiences where practical.

## Shared knowledge

A group may have:

- common knowledge
- common skills
- shared memory
- restricted/private memory

The simulation must make ownership and visibility explicit.

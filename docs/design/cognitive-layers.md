# Memory, Knowledge, Skills and Tools

## Ownership boundary

HWorld and HAgent have different responsibilities.

**HWorld owns the environment:** what exists, what is visible, what happens, and the authoritative results of physical actions.

**HAgent or another external cognitive system owns the mind:** what an agent remembers, believes, knows, learns, forgets, and decides.

HWorld must therefore expose useful events and observation/action contracts without becoming a second cognitive framework.

```text
HWorld
  world event / observation
          ↓
external cognition (for example HAgent)
  memory / knowledge / skills
          ↓
      decision
          ↓
HWorld action validation
```

## Do not call everything memory

An external cognitive system should distinguish at least four cognitive stores.

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

## HWorld responsibilities

HWorld may provide:

- authoritative world events
- observation snapshots
- action outcomes
- timestamps
- actor identity
- visibility/ownership rules
- persistence boundaries
- hooks for recording lineage or experiment data

It should not choose what a cognitive system remembers or believes.

## External cognitive responsibilities

HAgent or another cognitive implementation may provide:

- working memory
- episodic memory
- retrieval
- forgetting
- semantic/wiki-like knowledge
- reusable skills
- private/shared/group cognitive stores
- consolidation policies

These systems may be configured differently for different agents.

## Tool interaction

Memory, knowledge, skills, and world capabilities can be exposed through a general tool-call architecture used by HAgent.

```text
LLM
 -> tool request
 -> HAgent/tool execution
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

The world tools still terminate at HWorld validation; an LLM never directly mutates authoritative world state.

## Context budgeting

Do not send all memory every turn.

Use retrieval:

```text
large stores
 -> relevance filter
 -> top useful records
 -> compact context
```

The same information-economy principle used by perception should apply to cognition.

## Compression

External cognitive systems may use compact symbolic memory/knowledge formats so experiments can compare verbose natural language against compressed representations.

Example:

```text
O17=L12,D34,M
O22=R4,D12,S
O31=R28,D60,U
```

Experiment configuration should document what symbols mean rather than relying on hidden conventions.

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

Ownership and visibility must be explicit. HWorld should expose the relevant identity and visibility facts; the cognitive system determines how those facts become shared cognition.

## Generational inheritance

Generational inheritance belongs primarily to the external cognition experiment, while HWorld supplies parent/child relationships and environmental facts.

For example:

```text
World event:
object X harmed an agent

Ancestor cognition:
object X is dangerous

Child:
inherited belief: object X is dangerous

Later:
object X disappears

Across generations:
belief weight decays
```

This is a configurable experiment, not an assumption that inherited beliefs are biologically realistic.
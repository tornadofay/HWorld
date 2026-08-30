# Experimental Program

## E1 — Geometry-only navigation

One agent, unnamed objects, forward camera, no semantic world labels.

Measure whether the agent can navigate and form useful descriptions from geometry alone.

## E2 — Object naming

The engine gives only unknown object identities and shape/appearance descriptors. The agent creates its own names/concepts.

## E3 — Partial perception

Compare narrow, wide and omnidirectional cameras under the same world and goals.

## E4 — Latency competition

Run agents with different providers/models and intentionally different response delays. Measure how decision latency changes survival, exploration and interaction.

## E5 — Memory value

Compare:

- no memory
- episodic memory only
- episodic + semantic knowledge
- memory + wiki-like knowledge
- memory + skills

## E6 — Skill consolidation

Measure whether repeated experiences can become reusable procedures that reduce future reasoning cost.

## E7 — Two-agent unknown encounter

Neither agent is told that the other is an enemy, friend, or NPC. Study first-contact behavior.

## E8 — Human participant

A human enters the same world and can interact with AI agents through the same physical world rules.

## E9 — Generational inheritance

Study whether learned beliefs persist through generations and whether environmental disappearance causes forgetting.

## E10 — Co-evolution

Two populations affect each other's behavior and inherit adaptations. Observe whether stable cooperation, exploitation, avoidance or oscillation emerges.

## E11 — Token economy

Measure calls and estimated tokens for different sensor and context strategies.

## E12 — Sparse intelligence

Large populations use cheap local rules while only unusual situations invoke LLM reasoning.

## E13 — Renderer invariance

Run the same deterministic world through console and GDI+ and verify equivalent simulation outcomes.

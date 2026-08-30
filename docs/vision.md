# HWorld Project Vision

## 1. Vision

HWorld is a laboratory for persistent artificial worlds where humans and AI agents can coexist, perceive, act, remember, learn, cooperate, compete, form beliefs, develop skills, and change across generations.

The project should be usable at two levels at the same time:

1. **Experimenter level** — researchers/developers can inspect geometry, schedules, observations, tool calls, memory, costs, agent state, and deterministic replay.
2. **Player level** — a person who knows none of the underlying mathematics, programming, or AI can enter a world, configure an AI provider with their own API key, and play with or against the agents.

The project must therefore hide complexity without removing experimental control.

## 2. Central rule

**The simulation is not the renderer, and the renderer is not the world.**

A world can run without graphics. Graphics can visualize a world. A camera can observe a world without being the same thing as the visual renderer.

This allows the same simulation to run through:

- Console graphics
- WinForms GDI+
- Future Direct2D/DirectX
- Future Godot integration
- Future Unity integration
- Headless/server mode

## 3. Artificial life direction

The long-term subject is not merely AI gameplay. It is the study of emergent behavior under constrained perception, memory, tools, resources, interaction and inheritance.

Examples:

- An agent sees an unknown object and creates its own description for it.
- An agent observes another agent being harmed by an object and develops a fear or defensive rule.
- Descendants inherit some of that learned knowledge.
- The inherited knowledge weakens when the relevant object disappears for many generations.
- A formerly dangerous population changes behavior, begins interacting peacefully, and changes the dynamics of the world.
- Another population responds to the changed behavior and adapts in return.
- A human enters the same world and becomes another embodied participant rather than an external controller.

The project should allow these experiments without hard-coding their conclusions.

## 4. Cognitive model

HWorld should model multiple layers rather than calling every layer "memory":

```text
Perception
    -> Working context
    -> Episodic memory
    -> Semantic knowledge
    -> Skills/procedures
    -> Decisions
    -> Actions
    -> Consequences
    -> New experience
```

A family/group/population may additionally have shared knowledge, shared skills, or inherited tendencies.

## 5. Efficiency vision

External LLM calls are a scarce resource.

The world therefore uses event-driven and hierarchical cognition rather than requiring an LLM on every simulation tick.

Examples:

- Physics updates every tick.
- Most movement can be handled by deterministic behavior.
- A simple agent may react to routine events without an LLM.
- A difficult or novel event can wake the LLM.
- Memory retrieval returns only relevant items.
- Knowledge retrieval returns compact relevant facts.
- Tool results are structured and small.
- Camera observations are filtered before entering an LLM context.
- Agents can have different reasoning rates and providers.

## 6. Model diversity

Agents may use different:

- providers
- model families
- model versions
- reasoning settings
- cameras
- memory policies
- knowledge/skill stores
- behavioral priors
- action capabilities

A model is an interchangeable cognitive component, not the definition of an agent.

## 7. Hardware philosophy

The project must remain viable on systems without a powerful GPU.

The initial path is therefore:

**geometry -> console -> GDI+ -> optional vision model**

Actual image-based perception is an upgrade, not a prerequisite.

## 8. Long-term success

HWorld succeeds when the following becomes possible:

> Start a world, place several independent agents in it, give each a different body/sensor/brain/memory setup, allow time to advance continuously, add a human player, and observe behaviors that were not directly scripted.

The experimenter should be able to replay the world and inspect why each agent acted as it did.

# Generational Knowledge and Behavioral Inheritance

## Research idea

Agents can inherit selected information or behavioral tendencies from parents or groups.

This is deliberately broader than biological DNA. The project is studying the transmission of learned information, skills, beliefs and behavioral bias.

## Example

Agent A witnesses object X kill another agent.

A forms:

```text
belief: X is dangerous
response: avoid X
```

A later has descendants.

Those descendants may receive a weakened inherited belief:

```text
X dangerous: 0.7
```

If X remains present and descendants have confirming experiences, the belief may strengthen.

If X disappears for many generations, the inherited belief can decay:

```text
0.7 -> 0.55 -> 0.39 -> 0.22 -> 0.08
```

When the danger is forgotten, behavior changes.

## Counter-adaptation

Object X may itself be an evolving population.

Example:

```text
Population A:
fears X

Population X:
experiences avoidance by A

X descendants:
less aggression / more signaling

A descendants:
less fear / more cooperation

Result:
relationship changes over generations
```

The objective is not to force peace or hostility. The engine should make those outcomes possible without specifying the final behavior.

## Transmission modes

Experiments should support:

- no inheritance
- genetic-like trait inheritance
- direct memory inheritance
- cultural group transmission
- skill inheritance
- partial inheritance
- noisy/mutated inheritance
- decay over generations

## Provenance

An inherited fact should record its source when possible:

```text
origin agent/group
origin experience
generation
strength
confidence
last confirmation
last contradiction
```

This makes experiments inspectable rather than mysterious.

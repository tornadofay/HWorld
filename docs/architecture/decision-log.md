# Architecture Decision Log

## AD-001 — Renderer independence

**Decision:** World logic must not depend on GDI+, console APIs, Godot, Unity or another renderer.

**Reason:** A single simulation should support multiple visualizations and headless execution.

## AD-002 — GDI+ as first visual renderer

**Decision:** Use WinForms/GDI+ first.

**Reason:** It is available on the target development environment, requires no GPU-heavy engine and is excellent for debugging 2D geometry.

## AD-003 — Console as a first-class renderer

**Decision:** Build a console renderer during the early phases.

**Reason:** It validates the simulation independently of GUI technology and can later grow into a useful low-resource visual mode.

## AD-004 — Continuous simulation time

**Decision:** Do not make LLM requests the simulation clock.

**Reason:** Different model/provider latency must be measurable without freezing the world.

## AD-005 — Geometry perception before image perception

**Decision:** Begin with unnamed objects and compact geometry observations.

**Reason:** This is cheap, deterministic and allows perception/action/memory architecture to be tested before requiring a vision model.

## AD-006 — LLM is optional

**Decision:** The world must run without an LLM.

**Reason:** Most physical updates and routine behavior should not require external inference.

## AD-007 — HAgent remains separate

**Decision:** HAgent provides generic agent execution capabilities; HWorld owns world simulation.

**Reason:** Keeps both projects reusable and prevents environment-specific coupling.

## AD-008 — Generational knowledge is not literal DNA

**Decision:** Treat learned knowledge, skills and behavioral tendencies as configurable inheritance mechanisms.

**Reason:** The research question concerns transmission and decay of learned characteristics, not only biological genetics.

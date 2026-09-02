# HWorld engineering rules

This repository is designed to be worked on by human developers and coding agents.

## General Guidelines

* **Project Vision:** Please read the `README.md` carefully and ensure all contributions align with the vision of creating a modern, secure, and user-friendly.
* **Clarification:** If anything is unclear, **do not assume**—ask the user for clarification before proceeding.
* **Best Practices:** Always apply current best practices.
* **Front-end Design:** Maintain a modern, responsive, and professional UI consistent with best design principles.
* **Code Quality:** Ensure code adheres to modern conventions, prioritizing readability, scalability, maintainability, and security.

## WinForms UI conventions

1. Do not use `System.Windows.Forms.MessageBox` directly in `HWorld.WinForms`.
2. Use `HMessage.ShowDelete`, `ShowQuestion`, `ShowInformation`, `ShowError`, and `ShowException` for dialogs.
3. Use the shared HWorld `Header` for HWorld form chrome.
4. Use `HButton` for HWorld action buttons.

## Example and testing rules

5. `HWorld.Example` is the manual developer/verification host.
6. Every meaningful completed capability requires a matching Example verification using public APIs.
7. Keep Example code split across focused partial files/components.
8. Example snippets must be reproducible and explain required setup or shared setup.
9. Do not claim build/test success unless it was actually executed.

## Documentation rules

74. `README.md` is the public introduction and quick start.
75. `docs/architecture/` is the authoritative stable architecture description.
76. `docs/plan/` is implementation state: master direction, current state, and active implementation only.
77. `docs/roadmap/` is the ordered implementation path, including completed foundation history and future phases.
79. Root `plan.md` and `roadmap.md` are generated; do not hand-edit them except to synchronize a generated view when automation has not yet run.
80. When implementation changes architecture or milestone state, update the authoritative source document in the same change.
81. Do not duplicate architectural decisions across multiple source documents when a referenced authoritative document can own the decision.

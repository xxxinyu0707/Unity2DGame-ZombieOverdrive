# AI Collaboration Notes

This repository is a Unity single-player game assignment.

## Project Rules

- Unity project root should be `Game/`.
- Keep repository docs and helper scripts at the repo root.
- Do not commit Unity-generated cache folders such as `Library/`, `Temp/`, `Obj/`, `Logs/`, or build output.
- Keep Unity `.meta` files tracked.
- Prefer small playable slices over broad speculative architecture.

## Current Direction

Recommended prototype: 2D top-down collector.

Core loop:

- Move the player.
- Collect assignment fragments.
- Avoid obstacles.
- Reach the submission point before the timer ends.

## Useful Files

- `docs/ENVIRONMENT.md`: machine and Unity setup
- `docs/FIRST_UNITY_PROJECT.md`: exact Unity Hub project creation steps
- `docs/VIBE_CODING.md`: collaboration rhythm and prompt pattern
- `docs/GAME_BRIEF.md`: starting game concept
- `docs/TASKS.md`: phase checklist
- `scripts/check-env.ps1`: local setup verifier
- `scripts/open-unity-hub.ps1`: opens Unity Hub

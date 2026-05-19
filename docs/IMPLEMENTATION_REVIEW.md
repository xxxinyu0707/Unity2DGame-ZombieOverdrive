# Zombie Overdrive Implementation Review

## Feasibility

The design is feasible for a Unity/Tuanjie 2D course project, but the full document describes a medium-to-large roguelite rather than a tiny assignment prototype.

Recommended delivery strategy:

1. Build a playable vertical slice first.
2. Add weapons and enemy types one by one.
3. Treat evolution weapons, permanent progression, destructibles, and bosses as staged extensions.

## Difficulty Estimate

| Module | Difficulty | Notes |
| :--- | :--- | :--- |
| 2D movement, camera, mouse aim | Low | Straightforward and already scaffolded. |
| Basic auto weapon and bullets | Low-Medium | Object pooling is important but manageable. |
| Enemy spawning and pursuit | Medium | Needs tuning for density and performance. |
| XP drops and level-up choices | Medium | UI and game-state pausing need careful handling. |
| Six weapons with five levels each | High | Large content workload, best implemented incrementally. |
| Evolution weapons | High | Each evolved weapon is almost a separate feature. |
| Boss skills | High | Needs telegraphs, hitboxes, timing, and balancing. |
| Permanent meta progression | Medium | Technically simple, but requires save data and UI. |
| Infinite map wrapping | Medium | Can be faked early with a large tiled background. |
| 250 enemies on screen | Medium-High | Requires pooling and simple movement logic. |

## MVP Scope

The first playable version should include:

- WASD movement
- Mouse-directed auto pistol
- Smooth camera follow
- Walker, Runner, and Tanker enemies
- Time-based wave scaling
- XP crystal drops
- Level-up three-choice panel
- HP, XP, timer, kill count UI
- Pause, restart, game over, victory states
- Placeholder art generated inside Unity

This is enough to demonstrate the core loop:

```text
Move -> shoot -> kill -> collect XP -> level up -> survive longer
```

## Current Prototype Status

Implemented scaffold:

- `Assets/Scenes/Main.unity`
- Generated placeholder sprites
- Bullet, enemy, XP crystal prefabs
- Object pooling
- Player movement and health
- Pistol weapon
- Enemy chase behavior
- Wave spawner
- XP collection and level-up options
- HUD and upgrade panel

## Recommended Next Additions

1. Add shotgun as the second weapon.
2. Add enemy hit flash and death burst feedback.
3. Add audio placeholders for shooting, hit, pickup, level up.
4. Add a simple main menu and run result screen.
5. Add a proper 10-minute timeline with mid-run and final boss.
6. Add save data for permanent upgrades.

## Missing Design Details To Clarify Later

- Exact assignment rubric and deadline.
- Required platform for final build.
- Whether teacher expects original art or free assets are acceptable.
- Preferred visual style: pixel, clean vector, neon sci-fi, or cartoon zombie.
- Whether the final game should be in Chinese, English, or bilingual.
- Target run length for demonstration. A 3-5 minute demo mode may be better than the full 10 minutes for class presentation.

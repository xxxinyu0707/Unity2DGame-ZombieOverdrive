# Playtesting Guide

## Open Scene

Open:

```text
Game/Assets/Scenes/Main.unity
```

Press `Play` in the Tuanjie/Unity Editor.

## Controls

- `WASD`: move
- Mouse: aim all active weapons
- `ESC`: pause/resume
- `R`: restart after game over or victory

## What To Test First

1. Player moves smoothly.
2. Camera follows and slightly looks toward the mouse.
3. Pistol kills early walkers in about 1-2 hits.
4. The first few level-ups happen quickly after collecting crystals.
5. Leveling up heals the player a noticeable amount.
6. XP crystals are pulled in when near the player.
7. Level-up panel pauses the game and offers three choices.
8. Weapon choices can unlock/upgrade pistol, shotgun, tesla, black hole, lightblade, and laser.
9. Walker, Runner, Spitter, Tanker, and boss enemies appear as the timer advances.
10. HP, XP, level, timer, and kill count update.

## Useful Feedback Format

```text
测试到第几分钟：
发生了什么：
期望是什么：
Console 第一条红色错误：
截图：
```

## Current Prototype Limits

- Placeholder art only.
- The six weapons are implemented as playable first-pass versions, not final evolved forms.
- Spitter and bosses exist, but boss telegraphs and complex skill patterns are still simplified.
- The infinite map is currently a large visual placeholder, not full tile wrapping.
- Batch validation checks references, but true feel testing still needs manual Play mode.

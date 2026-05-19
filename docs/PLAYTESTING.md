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
3. Pistol fires automatically toward the mouse.
4. Green enemies spawn and chase the player.
5. Killing enemies drops blue XP crystals.
6. XP crystals are pulled in when near the player.
7. Level-up panel pauses the game and offers three choices.
8. Picking an upgrade resumes the game.
9. HP, XP, level, timer, and kill count update.

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
- Only pistol is implemented as a real weapon.
- Walker, Runner, and Tanker enemies exist; Spitter and Bosses are not implemented yet.
- The infinite map is currently a large visual placeholder, not full tile wrapping.
- Batch validation checks references, but true feel testing still needs manual Play mode.

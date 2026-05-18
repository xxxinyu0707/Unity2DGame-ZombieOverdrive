# Vibe Coding Workflow

Vibe coding here means: keep the game idea alive and playable while using AI as a fast pair programmer. We do not start by writing a giant design document. We build one small playable loop, test it, then grow it.

## Working Rhythm

1. Say the intended player experience in one sentence.
2. Pick the smallest playable slice.
3. Let AI draft or modify the scripts, scenes, prefabs, and checklists.
4. Run the game in Unity.
5. Describe what feels wrong or missing.
6. Iterate in small commits.

## Prompt Pattern

Use this shape when asking for implementation:

```text
目标：我想让玩家能 [do something].
当前状态：[what already exists in the Unity project].
约束：[2D/3D, keyboard/mouse, deadline, teacher requirements].
请你：直接修改项目文件，实现最小可玩的版本，并说明如何在 Unity 里验证。
```

Example:

```text
目标：做一个 2D 俯视角收集金币小游戏。
当前状态：Game/ 是空 Unity 2D 项目。
约束：单机，键盘移动，3 分钟内能玩明白。
请你：创建 PlayerMovement、Coin、GameManager 脚本，并告诉我场景怎么摆。
```

## AI Division Of Labor

Let AI handle:

- Script scaffolding and refactors
- Component wiring checklists
- Simple placeholder art plans
- Bug reproduction and fixes
- Build settings and submission checklist

Keep human control over:

- Core game concept
- What feels fun
- Teacher rubric priorities
- Final playtest judgment

## Commit Style

Commit after every playable improvement:

```text
feat: add player movement
feat: add coin collection loop
fix: keep camera inside map bounds
chore: configure Unity serialization
```

## First Playable Slice

For a safe course-project scope, start with one of these:

- 2D top-down collector: move, collect, timer, score, win/lose
- 2D platformer: move, jump, hazards, checkpoint, goal
- Grid puzzle: move pieces, undo, level clear
- Survival arena: move, dodge enemies, survive timer

Recommended first choice: 2D top-down collector. It is fast to build, easy to polish, and friendly to future extensions like power-ups, enemies, UI, sound, and level design.

## Definition Of Done

A feature is done when:

- It works in Play Mode.
- The scene can be reopened without missing references.
- Console has no red errors.
- The behavior can be explained in one sentence.
- There is a screenshot or short note showing how to verify it.

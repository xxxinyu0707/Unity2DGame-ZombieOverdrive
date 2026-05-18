# First Unity Project Steps

Follow this after Unity 6.3 LTS is installed in Unity Hub.

## Create The Project

1. Open Unity Hub.
2. Choose `New project`.
3. Template: `2D`.
4. Project name: `Game`.
5. Location: `D:\学术垃圾\大三下\Unity大作业`.
6. Confirm the final path is:

```text
D:\学术垃圾\大三下\Unity大作业\Game
```

## Configure Editor Settings

In Unity:

1. Open `Edit > Project Settings > Editor`.
2. Set `Version Control Mode` to `Visible Meta Files`.
3. Set `Asset Serialization Mode` to `Force Text`.
4. Open `Edit > Preferences > External Tools`.
5. Set the external script editor to VS Code or Visual Studio.

## Suggested Scene Baseline

For the first playable test:

- Rename the default scene to `Main`.
- Create folders under `Assets/`:
  - `Scenes`
  - `Scripts`
  - `Prefabs`
  - `Sprites`
  - `Audio`
  - `Materials`
- Move `Main.unity` into `Assets/Scenes`.

## After Creating The Project

Back in PowerShell:

```powershell
.\scripts\check-env.ps1
git status --short
```

Expected tracked folders from Unity:

- `Game/Assets`
- `Game/Packages`
- `Game/ProjectSettings`

Do not commit `Game/Library`, `Game/Temp`, or build output folders.

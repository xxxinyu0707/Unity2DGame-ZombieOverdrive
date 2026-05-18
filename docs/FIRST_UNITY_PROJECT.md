# First Tuanjie/Unity Project Steps

Follow this after Tuanjie Engine is installed in Tuanjie Hub.

## Create The Project

1. Open Tuanjie Hub from PowerShell, so its cache and temp paths stay on D drive:

```powershell
.\scripts\open-tuanjie-hub.ps1
```

2. In Hub settings, make sure these paths are on D drive:
   - Editor path: `D:\Tuanjie\Hub\Editor`
   - Download/cache path: `D:\Tuanjie\Hub\Downloads`
3. Choose `New project`.
4. Template: `2D`.
5. Project name: `Game`.
6. Location: `D:\学术垃圾\大三下\Unity大作业`.
7. Confirm the final path is:

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

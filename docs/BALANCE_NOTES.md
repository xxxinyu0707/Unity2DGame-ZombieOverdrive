# Difficulty Balance Notes

This pass uses `scripts/balance-model.ps1` to model the run as a closed loop instead
of assuming perfect upgrade choices.

## Model Assumptions

- 10 minute run.
- 3 upgrade choices per level.
- 3 active weapon slots and 3 passive skill slots.
- 3 global rerolls per run.
- Evolution requires the active weapon at level 5 and the matching passive at level 5.
- A mildly rational player prefers owned weapons, matching passives, and ready evolutions,
  but still has noisy/random choice behavior.
- Every simulated second recomputes:
  - current build DPS,
  - average enemy HP and XP from the active wave mix,
  - kill rate,
  - collected XP,
  - level-ups and chosen upgrades,
  - the next second's DPS from the updated build.

## 1200-Run Monte Carlo Snapshot

| Curve | Final level p25/med/p75 | Median kills | First evolution p25/med/p75 | No evolution |
| --- | --- | --- | --- | --- |
| Baseline | 34 / 38 / 39 | 2805 | 04:48 / 06:02 / 06:03 | 1.5% |
| Current | 34 / 40 / 42 | 2758 | 04:47 / 05:39 / 06:02 | 1.6% |

## Pressure Curve

`combat_pressure` is the estimated spawned enemy HP per second divided by current
build DPS. Around `1.0` means the wave is roughly matching the player's damage output.

| Minute | Baseline combat pressure median | Current combat pressure median |
| --- | --- | --- |
| 1 | 0.43 | 0.44 |
| 2 | 0.35 | 0.36 |
| 3 | 0.40 | 0.43 |
| 4 | 0.38 | 0.42 |
| 5 | 0.66 | 0.75 |
| 6 | 0.68 | 0.64 |
| 7 | 0.71 | 0.78 |
| 8 | 0.64 | 0.93 |
| 9 | 0.98 | 1.31 |
| 10 | 0.98 | 1.40 |

The current curve intentionally leaves minutes 1-4 close to baseline, then raises
pressure after minute 5, where most normal runs are approaching or entering their
first evolution window.

## Implemented Changes

- Kept the XP requirement curve unchanged.
- Increased enemy HP scaling mildly over the whole run.
- Added a stronger fixed late-run HP ramp after minute 5.
- Raised the enemy cap and spawn cadence mostly from minute 4 onward.
- Shifted minute 5+ composition toward more runner/spitter/tanker pressure.
- Did not scale enemies from the player's evolved weapon state; the curve is still
  time-based rather than punitive.

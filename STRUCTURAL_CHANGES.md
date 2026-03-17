# TeamArena Structural Changes — March 2026

This document records the architectural and logic changes made across the TeamArena
codebase. Each section explains what existed before, what changed, and why.

---

## 1. Avatar Auto-Matching Service

### Files Changed
- **NEW** `Services/AvatarResolver.cs` — static utility for name-to-avatar resolution
- `Services/MockDataService.cs` — calls `AvatarResolver.Resolve()` after initialization
- `GameScoreboard.Server/Controllers/SeedController.cs` — `CreatePod()` uses local `ResolveAvatar()` instead of hardcoded `adam1.png`
- `Pages/DepartmentOverview.razor` — fallback changed from `?? "images/avatars/adam1.png"` to `AvatarResolver.Resolve()`
- `Pages/PodOverview.razor` — same fallback change

### Before
Pod members in both MockDataService and SeedController were assigned generic placeholder
avatars (`adam1.png`, `kla1.png`, etc.) regardless of whether a real photo existed for
that person. The mapping was fully manual.

### After
`AvatarResolver` performs a three-tier lookup: explicit override map (for edge cases
like "David Schunk" mapping to `david shunk.jpg`), full-name filename match, then
first-name filename match. A `ResolveAllAvatars()` post-init step in MockDataService
and a `ResolveAvatar()` helper in SeedController ensure all members get the best
available photo. New photos only require dropping a file into `wwwroot/images/avatars/`
with a name that matches.

### Why
The team kept adding new headshots but members still showed generic placeholders.
Manual assignment was error-prone and required code changes per person.

---

## 2. Performance Scoring Engine (Weighted)

### Files Changed
- `Pages/CharacterDetails.razor` — `CalculatePerformanceScore()`, `GetTotalPerformanceScore()`, new `_podMetricWeights` dictionary, new `ComputeWeightedScore()`

### Before
- `CalculatePerformanceScore()` used a hardcoded `switch` statement for goal targets,
  ignoring per-member goals set via `GetMemberGoal()`.
- All metrics were equally weighted when computing the total performance score
  (simple arithmetic mean).
- Surveys/5Star = 0 produced a score of 0, which when averaged with other metrics
  would catastrophically drag down the overall score.

### After
- `CalculatePerformanceScore()` now calls `GetMemberGoal()` for per-person or
  pod-default targets. Falls back to team average only when no goal exists.
- A peer-comparison adjustment of +/-5 points is applied based on whether the
  member is above or below the team average, so two members at the same goal
  percentage are distinguishable.
- `GetTotalPerformanceScore()` uses a weighted average for pod members:
  - RPH: 7/32 (21.875%)
  - AppEff: 7/32 (21.875%)
  - PMEff: 5/32 (15.625%)
  - AccAttach: 5/32 (15.625%)
  - WarrantyAttach: 5/32 (15.625%)
  - Surveys/5Star: 3/32 (9.375%)
- Regular department members still use equal weighting.

### Why
Management provided explicit metric weights. The old equal-weight average over-valued
minor metrics (5-Star) and under-valued primary revenue drivers (RPH, AppEff). The
hardcoded goals in the scoring function were a bug that ignored personal goal overrides.

---

## 3. 5-Star / Surveys = 0 Handling

### Files Changed
- `Pages/CharacterDetails.razor` — `CalculatePerformanceScore()`

### Before
A Surveys value of 0 returned a performance score of 0. Since the total was a
simple average, this single zero pulled the entire score down dramatically.

### After
Surveys/5Star = 0 now returns a score of 15 (small penalty). This still reflects
negatively but does not obliterate the member's overall score. The metric still
shows red in the progress bar and appears as a weak area on the radar chart because
the 15-point score is far below the ~80-100 range of healthy metrics.

### Why
A zero 5-Star score means the member had no surveys recorded, not necessarily that
they performed badly. The minimal penalty preserves the "needs attention" signal
without unfairly tanking the total performance score.

---

## 4. Overall Rank (Performance-Based)

### Files Changed
- `Pages/CharacterDetails.razor` — `GetOverallRank()`, new `ComputePerformanceScoreForMember()`

### Before
Overall Rank was determined by `TotalExperience` (XP), which is a cumulative lifetime
metric and does not reflect current-period performance.

### After
Rank is computed by comparing each department member's weighted performance score
(identical to `GetTotalPerformanceScore()`) against the current member. A helper
`ComputePerformanceScoreForMember()` builds any member's metrics dict from
`_allLatestPeriodDeptRecords` and runs `ComputeWeightedScore()` to ensure scoring
is consistent.

### Why
XP-based ranking rewarded tenure over performance. A member who was hired first
would always outrank a newer member even if the newer member was outperforming.

---

## 5. Progress Bar Color Tiers (4-Tier System)

### Files Changed
- `Pages/CharacterDetails.razor` — `GetMetricBarCssClass()`, `GetMetricBorderColor()`, `GetMetricValueColor()`, new `IsAtOrAboveGoal()`
- `wwwroot/css/elden-ring.css` — new `.bar-dim` class

### Before
Three tiers existed:
- Gold (default) — top performer
- Green — above average
- Red — bottom performer

The problem: members who were below average AND below goal fell through to the
default gold styling, giving a false impression of elite performance. Example:
Seyquan's AppEff was below average and below goal, yet showed gold.

### After
Four tiers:
- **Gold** (default gradient) — #1 in department for this metric
- **Green** (`bar-green`) — above team average OR meeting/exceeding personal goal
- **Dim** (`bar-dim`) — below average AND below goal (needs work)
- **Red** (`bar-red`) — worst in department

The new `IsAtOrAboveGoal()` helper checks the member's value against their personal
goal (respecting lower-is-better inversion for efficiency metrics). Border and text
colors follow the same 4-tier logic.

### Why
The original 3-tier system had a logical gap where underperformers appeared gold.
Adding `bar-dim` and the goal-awareness check closes the gap.

---

## 6. Champion Stats Accuracy

### Files Changed
- `Models/TeamMember.cs` — `GetStrongestMetricRelativeToTeam()`

### Before
`GetStrongestMetricRelativeToTeam()` always used `value / average` to compute
relative strength. For lower-is-better metrics (AppEff, PMEff, Awk), this meant
a WORSE value produced a HIGHER strength score, incorrectly identifying a poor
efficiency metric as the member's strongest area.

### After
The method now checks a `_lowerIsBetterMetrics` set. For those metrics, the ratio
is inverted to `average / value`, so a lower value (better) produces a higher
relative-strength score. This correctly identifies the member's true strongest area.

### Why
Champion Stats "Strongest Area" was misleading for pod members whose best metric
happened to be an efficiency one. The inverted ratio was a straightforward bug.

---

## 7. Today's Quest Panel — Off-Track Styling

### Files Changed
- `Pages/CharacterDetails.razor` — `DailyShiftGoals` record (expanded), `ComputeTodayShiftGoals()`, Quest panel HTML
- `wwwroot/css/elden-ring.css` — new `.quest-metric-buffed`, `.quest-metric-on-track`, `.quest-exclamation` classes

### Before
The Today's Quest panel displayed shift goals with a static label `(buffed +20%)`
regardless of actual buff status. There was no visual distinction between on-track
and off-track metrics.

### After
- The `DailyShiftGoals` record now tracks `bool ...Buffed` and `int ...BuffPercent`
  for each metric (RPH, App, PM, Warranty).
- If any metric is buffed (multiplier > 1.01), the panel border turns red and shows
  "Behind pace — targets buffed to catch up."
- Each buffed metric row gets a red left-border, a bold red `!` exclamation icon,
  and shows the exact buff percentage: `(buffed +23%)`.
- On-track metrics show no special styling.
- The `else` block now only renders for pod members (`else if (IsPodMember())`),
  preventing an empty quest panel on department-only members.

### Why
The quest panel was purely informational with no visual urgency. The user requested
negative-connotation styling to indicate when someone is behind and needs the buff
to catch up.

---

## 8. Panel Position Swap

### Files Changed
- `Pages/CharacterDetails.razor` — right sidebar HTML structure

### Before
Panel order (top to bottom):
1. Champion Stats
2. Radar Chart
3. Today's Quest

### After
Panel order (top to bottom):
1. Today's Quest
2. Radar Chart
3. Champion Stats

### Why
Today's Quest is the most actionable daily information — it tells the member what
they need to hit today. Placing it at the top ensures it is seen first. Champion
Stats is a summary/retrospective panel better suited to the bottom position.

# Tracker Import Logic

## Current import shape

The pod tracker paste expects the Best Buy dashboard export with:

- a global header row containing `Employee`, `Rev Per Hour`, `Apps / App Eff`, `PMs / PM Eff`, `Surveys / 5-Star`, `Warranty Attach`, and `Dept Specific`
- a pod header row for each team that contains the repeated `Actual / Goal / Track` bands
- employee rows beneath each pod header

The parser now derives metric columns from that structure instead of relying on brittle fixed indexes.

## Core parsing rules

- `RPH` comes from the `Rev Per Hour` actual column.
- `AppEff` comes from the `Apps / App Eff` section's efficiency value, not the app count or goal.
- `PMEff` comes from the `PMs / PM Eff` section's efficiency value.
- `Surveys` comes from the `Surveys / 5-Star` actual value.
- `WarrantyAttach` comes from the `Warranty Attach` actual value.
- Department-specific columns are read from the pod header row labels:
  - PC teams use `PCBasket` and `Office`
  - HT teams use `HTBasket` and `ServAttach`

## Matching and stale-data protections

- Current-team matching can use exact and relaxed matching.
- Cross-team matching is intentionally stricter and does not allow first-name-only fallback.
- When an existing member is matched in another pod, the import marks that member as moved and persists the department update during save.
- Ambiguous matches are logged and skipped instead of silently binding the row to the wrong person.

## Public vs internal pod metrics

- Internal pod scoring and saves can still use department-specific metrics.
- Public-facing pod overview, character details, and Focus 5 only show the shared pod KPIs:
  - `RPH`
  - `AppEff`
  - `PMEff`
  - `Surveys`
  - `WarrantyAttach`

This prevents `HTBasket`, `ServAttach`, `PCBasket`, and `Office` from leaking into overview cards and character pages.

## Tests

`GameScoreboard.Tests` currently covers:

- dashboard-format parsing for warranty and department-specific values
- stale-name protection for cross-pod matching
- pod metric visibility/applicability rules
- `MetricsController.Save()` replacement behavior for member-period metric sets

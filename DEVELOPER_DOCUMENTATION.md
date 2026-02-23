# Team Performance Arena - Developer Documentation

> **Last Updated:** February 2026  
> **Framework:** Blazor WebAssembly (.NET 8) with ASP.NET Core Web API Backend  
> **Live Site:** https://drew-834.github.io/TeamArena/  
> **API:** Azure App Service (ASP.NET Core Web API + SQL Server)

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture Overview](#architecture-overview)
4. [Feature Documentation](#feature-documentation)
5. [Key Components Reference](#key-components-reference)
6. [Data Models](#data-models)
7. [Services & State Management](#services--state-management)
8. [Developer Notes & How-Tos](#developer-notes--how-tos)
9. [TODO List](#todo-list)
10. [Potential Issues & Bugs](#potential-issues--bugs)
11. [Deployment Notes](#deployment-notes)

---

## Project Overview

Team Performance Arena is a gamified performance visualization dashboard that presents retail team metrics in an engaging, video game-inspired interface (Elden Ring aesthetic). The application tracks employee performance across multiple departments and visualizes metrics through character cards, radar charts, and leaderboards.

### Core Concept

- Team members displayed as "champions" with game-inspired titles
- Performance metrics drive an XP/leveling system
- Department-based organization (Computers, Store, Front End, Warehouse)
- Bi-weekly tracking periods (Mid-month and End-of-Month)

---

## Technology Stack

| Technology | Purpose |
|------------|---------|
| **Blazor WebAssembly** | Frontend framework (client-side C#) |
| **.NET 8** | Runtime target |
| **Tailwind CSS** | Styling (via CDN) |
| **Entity Framework Core 9.0** | Data access (SQLite configured but using MockDataService) |
| **LocalStorage/SessionStorage** | Client-side persistence for terms acceptance |
| **ASP.NET Core Web API** | Backend API (GameScoreboard.Server project) |
| **Entity Framework Core (SQL Server)** | Server-side data persistence (Azure SQL) |
| **GitHub Pages** | Frontend hosting |
| **Azure App Service** | API hosting |

### Key Dependencies (from `GameScoreboard.csproj`)

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.3" />
```

---

## Architecture Overview

```
GameScoreboard/
├── Pages/                    # Routable page components
│   ├── Index.razor           # Department selection (route: "/")
│   ├── DepartmentOverview.razor # Department dashboard (route: "/departments/{DepartmentName}")
│   ├── CharacterDetails.razor   # Individual member view (route: "/character/{Id:int}")
│   ├── WeeklyTracker.razor      # Admin data entry (route: "/weeklytracker")
│   ├── ArchivedTrackers.razor   # Historical data view (route: "/archived-trackers")
│   ├── RadarChart.razor         # Radar chart SVG component
│   ├── PodIndex.razor           # Pod selection (route: "/pods")
│   ├── PodOverview.razor        # Pod KPI dashboard (route: "/pods/{PodName}")
│   └── PodTracker.razor         # Pod admin data entry (route: "/podtracker")
│
├── Shared/                   # Reusable components
│   ├── MainLayout.razor      # App layout with terms modal
│   ├── TermsOfUseModal.razor # Terms acceptance modal
│   ├── NavMenu.razor         # Navigation (largely unused)
│   ├── CharacterCard.razor   # Empty stub (cards inline in pages)
│   └── MetricDisplay.razor   # Empty stub
│
├── Models/
│   ├── TeamMember.cs         # Core team member entity
│   ├── RankedMember.cs       # DTO for rankings display
│   └── PerformanceMetric.cs  # Empty stub
│
├── Services/
│   ├── MockDataService.cs    # In-memory data implementation + IDataService interface
│   ├── AppState.cs           # Cross-component event bus
│   ├── DataService.cs        # Empty stub
│   └── DepartmentSummary.cs  # Summary calculation helper
│
├── Data/
│   └── AppDbContext.cs       # EF Core context + MetricRecord entity
│
└── wwwroot/
    ├── index.html            # Host page with Tailwind CDN
    ├── css/app.css           # Custom styles
    ├── js/app.js             # Carousel & key detection JS
    └── images/avatars/       # Team member avatars
```

---

## Feature Documentation

### 1. Department Selection (Index.razor)

**Route:** `/`

**Description:** Landing page with department selection cards. Features a hidden "Weekly Tracker" card revealed by pressing `.` three times.

**Key Features:**
- Department cards: Store, Front End, Computers, Warehouse
- Hidden Easter egg: Press `.` three times to reveal Weekly Tracker link
- Navigation to department-specific dashboards

**Developer Notes:**
- Hidden card uses `id="weekly-tracker-card"` with `hidden` class
- Key detection implemented in `wwwroot/js/app.js` via `initKeySequenceDetector()`
- Called in `OnAfterRenderAsync()` on first render

```csharp
// To modify departments, update the cards in Index.razor
// Navigation uses: NavigationManager.NavigateTo($"/departments/{departmentName}")
```

---

### 2. Department Overview (DepartmentOverview.razor)

**Route:** `/departments/{DepartmentName}`

**Description:** Shows department-wide metrics summary and team member character cards.

**Key Features:**
- Metric summary cards with averages, highs, lows
- Progress bars showing performance vs goals
- Team member cards showing strongest area
- XP-based leveling displayed on cards
- Click-through to individual character details

**Developer Notes:**
- Subscribes to `AppState.OnMetricsUpdatedAsync` to refresh when WeeklyTracker saves
- Uses `IDisposable` to unsubscribe on component disposal
- Goals are hardcoded in `GetMetricGoal()` method:

```csharp
private double? GetMetricGoal(string metricKey) => metricKey switch
{
    "PMAttach" => 25.0,
    "M365Attach" => 30.0,
    "GSP" => 20.0,
    "Revenue" => 75000.0,
    "ASP" => 600.0,
    "Basket" => 150.0,
    _ => null
};
```

**To Add New Metrics:**
1. Add metric key to `_departmentMetrics` in `WeeklyTracker.razor`
2. Add display name in `TeamMember.GetMetricDisplayName()`
3. Optionally add goal in `GetMetricGoal()` methods (exists in multiple files)

---

### 3. Character Details (CharacterDetails.razor)

**Route:** `/character/{Id:int}`

**Description:** Detailed view of a single team member with all metrics, radar chart, and comparative stats.

**Key Features:**
- Performance metrics with visual bars
- Team average comparison markers
- Color-coded indicators (gold=best, red=worst, blue=above avg)
- Custom SVG radar chart showing 6 key metrics
- Champion stats panel (rank, strongest area, etc.)
- Background avatar effect

**Developer Notes:**
- Metrics data comes from `IDataService.GetMetricRecordsAsync()`
- Performance scoring includes bonus multiplier (1.25x) for exceeding goals
- Radar chart data generated via `GenerateRadarChartData()` using normalized scores

**Performance Score Algorithm:**
```csharp
// Higher is better:
double baseRatio = numericValue / baseScoreTarget;
score = baseRatio * 100;
if (numericValue > baseScoreTarget) {
    double bonusPoints = ((excessValue / baseScoreTarget) * 100) * 1.25;
    score = 100 + bonusPoints;
}
// Capped at 150
```

---

### 4. Weekly Tracker (WeeklyTracker.razor)

**Route:** `/weeklytracker`

**Description:** Admin interface for entering bi-weekly performance metrics. Protected by password.

**Key Features:**
- Password protection (hardcoded: `Admin321`)
- Department & period selection dropdowns
- CSV/Excel paste functionality for bulk data entry
- Data input grid per team member
- XP calculation and ranking updates
- Real-time department rankings panel

**Developer Notes:**

**Password Location:**
```csharp
private const string CorrectPassword = "Admin321"; // Line 225
```

**Period Format:** Periods use format `Mid-MMM yyyy` or `EOM-MMM yyyy` (e.g., "Mid-Jan 2026", "EOM-Jan 2026")

**CSV Column Mapping:**
```csharp
private readonly Dictionary<string, string> _internalKeyToCsvHeaderMap = new()
{
    { "M365Attach", "M365 Attach" },
    { "GSP", "Total Warranty Attach" },
    { "Revenue", "Category Direct $" },
    { "ASP", "ASP" },
    { "Basket", "Basket RPT" },
    { "PMAttach", "PM Attach" }
};
```

**To Add New Departments/Metrics:**
1. Add to `_departmentMetrics` dictionary
2. Update `_internalKeyToCsvHeaderMap` for CSV import support
3. Add name matching aliases in `HandlePastedData()` if needed

**XP Calculation:**
```csharp
// Average of all metric scores for the period
double averagePeriodScore = currentPeriodPerformanceScoreTotal / currentPeriodMetricCount;
member.TotalExperience += averagePeriodScore;
```

---

### 5. Radar Chart (RadarChart.razor)

**Description:** Custom SVG-based radar/spider chart component.

**Parameters:**
```csharp
[Parameter] public List<string>? Labels { get; set; }
[Parameter] public List<double>? NormalizedData { get; set; } // 0-100 scale
[Parameter] public string Width { get; set; } = "250px";
[Parameter] public string Height { get; set; } = "250px";
```

**Developer Notes:**
- Pure SVG rendering, no external libraries
- 5 concentric grid rings representing 20/40/60/80/100%
- Data expected as normalized 0-100 scores

---

### 6. Pod Performance System (PodIndex, PodOverview, PodTracker)

**Routes:** `/pods`, `/pods/{PodName}`, `/podtracker`

**Description:** Separate performance tracking system for pods (cross-functional teams) with different KPIs than the department system. Uses weighted scoring.

**Pod KPIs & Weights (Total: 32 parts):**

| Metric Key | Display Name | Weight | Target | Direction |
|------------|-------------|--------|--------|-----------|
| `AppEff` | App Eff $ | 7 | $6,500 | Lower is better |
| `RPH` | RPH $ | 7 | $920 | Higher is better |
| `WarrantyAttach` | Warranty % | 5 | 10% | Higher is better |
| `AccAttach` | Acc Attach % | 5 | 20% | Higher is better |
| `PMEff` | PM Eff $ | 5 | $4,000 | Lower is better |
| `Surveys` | 5-Star | 3 | 4.6 | Higher is better |

**Current Pods:**
- Matt-Category Advisors (10 members)
- Luis the Beast (11 members)
- Drews Crew-Computing (13 members)
- Pod-Front End (8 members)

**Weighted Score Calculation:**
```csharp
// For each metric: normalize to 0-150 scale relative to target
// "Higher is better": normalizedScore = (value / target) * 100
// "Lower is better" (AppEff, PMEff): normalizedScore = (target / value) * 100
// Final: sum(normalizedScore * weight) / sum(applicableWeights)
```

**Excel Paste Format:** PodTracker accepts copy-paste from the pod performance Excel spreadsheet. It auto-detects tab-delimited columns and maps: Employee, RPH, App Eff (value), PM Eff (value), Surveys/5-Star, Warranty Attach, Accessory Attach.

**Adding a New Pod:**
1. Add members with the pod name as `Department` in `MockDataService`
2. Add the pod name to `PodDepartments` HashSet in `PodIndex.razor`, `PodOverview.razor`, and `PodTracker.razor`
3. Add metrics definition in `WeeklyTracker._departmentMetrics` (optional, for compatibility)

---

### 7. Terms of Use Modal (TermsOfUseModal.razor + MainLayout.razor)

**Description:** Modal requiring user acceptance before viewing content.

**Implementation:**
- Uses `sessionStorage` to track acceptance per session
- Located in `MainLayout.razor` which wraps all pages
- Blur effect applied to content when modal is visible

---

### 8. Hidden Features & Easter Eggs

| Feature | Trigger | Effect |
|---------|---------|--------|
| Weekly Tracker Link | Press `.` three times | Reveals purple card on Index page |

---

## Key Components Reference

### MainLayout.razor
- Wraps all page content
- Manages terms acceptance state
- Contains footer with copyright

### App.razor
- Router configuration
- 404 handling with custom styled page

---

## Data Models

### TeamMember (Models/TeamMember.cs)

```csharp
public class TeamMember
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public string? Role { get; set; }
    public string? AvatarUrl { get; set; }
    public double TotalExperience { get; set; } = 0;
    public ICollection<MetricRecord> MetricRecords { get; set; }
    
    // Computed properties:
    public int CurrentLevel => (int)(TotalExperience / 100) + 1;
    public double ProgressToNextLevelPercentage => (TotalExperience % 100);
}
```

### MetricRecord (Data/AppDbContext.cs)

```csharp
public class MetricRecord
{
    public int TeamMemberId { get; set; }          // Composite key part 1
    public required string Period { get; set; }    // Composite key part 2
    public required string MetricKey { get; set; } // Composite key part 3
    public string? Value { get; set; }             // Stored as string
    public TeamMember TeamMember { get; set; }
}
```

### RankedMember (Models/RankedMember.cs)

```csharp
public class RankedMember
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public double Score { get; set; }   // TotalExperience used for ranking
    public int Rank { get; set; }
}
```

---

## Services & State Management

### IDataService Interface

```csharp
public interface IDataService
{
    Task<List<TeamMember>> GetTeamMembersAsync(string? department = null);
    Task<TeamMember?> GetTeamMemberByIdAsync(int id);
    Task AddTeamMemberAsync(TeamMember member);
    Task UpdateTeamMemberAsync(TeamMember member);
    Task DeleteTeamMemberAsync(int id);
    Task SaveMetricRecordsForPeriodAsync(int memberId, string period, List<MetricRecord> records);
    Task<List<MetricRecord>> GetMetricRecordsAsync(int? memberId = null, string? period = null);
}
```

### MockDataService
- In-memory implementation of `IDataService`
- Pre-populated with sample team members
- Uses Dictionary with compound key `(MemberId, Period)` for metric storage
- **Note:** Data is lost on page refresh (no persistence)

### AppState (Services/AppState.cs)
- Singleton service for cross-component communication
- Event: `OnMetricsUpdatedAsync` - fired when WeeklyTracker saves data
- Used by DepartmentOverview to refresh when data changes

```csharp
// Publishing:
await AppState.NotifyMetricsUpdatedAsync();

// Subscribing:
AppState.OnMetricsUpdatedAsync += HandleMetricsUpdated;
// Don't forget to unsubscribe in Dispose()!
```

---

## Developer Notes & How-Tos

### Adding a New Team Member

1. In `MockDataService.InitializeMockData()`, add:
```csharp
new TeamMember
{
    Id = [next_id],
    Name = "New Person",
    Department = "Computers", // or other department
    AvatarUrl = "images/avatars/newperson.png",
    MetricRecords = new List<MetricRecord> { ... }
}
```

2. Add avatar image to `wwwroot/images/avatars/`

### Adding a New Department

1. Add members with new department name in `MockDataService`
2. Add department card in `Index.razor`
3. Add metrics definition in `WeeklyTracker._departmentMetrics`
4. Add CSV header mappings if needed in `_internalKeyToCsvHeaderMap`

### Adding a New Metric

1. **Display Name:** Update `TeamMember.GetMetricDisplayName()`
2. **Title (for champions):** Update `TeamMember.MetricTitles` dictionary
3. **Department Association:** Add to relevant department in `WeeklyTracker._departmentMetrics`
4. **Goal (optional):** Add to `GetMetricGoal()` methods in:
   - `DepartmentOverview.razor`
   - `CharacterDetails.razor`
   - `WeeklyTracker.razor`
5. **Radar Chart:** If adding to radar, update `CharacterDetails.GenerateRadarChartData()`

### Changing the Admin Password

Edit `WeeklyTracker.razor`:
```csharp
private const string CorrectPassword = "NewPassword123";
```

### Modifying XP/Leveling System

Current formula: 100 XP per level, Level = (TotalXP / 100) + 1

To modify, update in `TeamMember.cs`:
```csharp
public int CurrentLevel => (int)(TotalExperience / YOUR_XP_PER_LEVEL) + 1;
```

---

## TODO List

### High Priority

| ID | Task | Location | Notes |
|----|------|----------|-------|
| T01 | Implement real data persistence | `MockDataService.cs` | Currently all data is in-memory and lost on refresh. Need backend API or IndexedDB |
| T02 | Replace hardcoded admin password | `WeeklyTracker.razor:225` | Security risk - implement proper authentication |
| T03 | Implement ArchivedTrackers functionality | `ArchivedTrackers.razor` | Page is a placeholder with no actual functionality |
| T04 | Add data export functionality | `WeeklyTracker.razor` | README mentions export capabilities but not implemented |
| T05 | Complete EfDataService implementation | `Program.cs:27` | EF Core is configured but using MockDataService. SQLite won't work in WASM without special handling |

### Medium Priority

| ID | Task | Location | Notes |
|----|------|----------|-------|
| T06 | Add CSV mapping for Store/Front/Warehouse departments | `WeeklyTracker._internalKeyToCsvHeaderMap` | Only Computers department has mappings |
| T07 | Implement toast notifications for save feedback | `WeeklyTracker.razor:14` | TODO comment present in code |
| T08 | Add metric definitions from config | `WeeklyTracker.razor:249` | TODO comment - metrics hardcoded, should be configurable |
| T09 | Remove unused NavMenu/Counter/FetchData pages | `Shared/NavMenu.razor`, `Pages/` | Default Blazor template remnants |
| T10 | Complete empty stub classes | Multiple | `CharacterCard.cs`, `MetricDisplay.cs`, `PerformanceMetric.cs`, `DataService.cs` |
| T11 | Implement week-over-week comparison view | `CharacterDetails.razor` | Mentioned in README but not visible in UI |
| T12 | Add proper role/title system | `TeamMember.cs` | Role field exists but isn't populated or used |

### Low Priority / Enhancements

| ID | Task | Location | Notes |
|----|------|----------|-------|
| T13 | Add achievement badge system | - | Mentioned in README future enhancements |
| T14 | Create mobile companion app | - | Mentioned in README future enhancements |
| T15 | Add AI performance predictions | - | Mentioned in README future enhancements |
| T16 | Implement team vs team comparisons | - | Mentioned in README future enhancements |
| T17 | Add PDF/Excel report generation | - | Mentioned in README future enhancements |

---

## Potential Issues & Bugs

### Critical Issues

| ID | Issue | Location | Status | Description |
|----|-------|----------|--------|-------------|
| B01 | Data loss on refresh | `MockDataService.cs` | ⚠️ COMMENTED | All entered data is stored in-memory only. Refreshing the page loses all tracked metrics. |
| B02 | SQLite won't work in WASM | `Program.cs:18` | ⚠️ COMMENTED | `AppDbContext` is configured but SQLite requires server-side or special WASM-compatible approach (e.g., sql.js) |
| B03 | Hardcoded password visible in source | `WeeklyTracker.razor:225` | ⚠️ COMMENTED | Password "Admin321" is in client-side code |

### Moderate Issues

| ID | Issue | Location | Status | Description |
|----|-------|----------|--------|-------------|
| B04 | Code duplication in helper methods | Multiple files | ⚠️ COMMENTED | `GetDoubleValue()`, `FormatMetricValue()`, `GetMetricGoal()` duplicated across CharacterDetails, DepartmentOverview, WeeklyTracker |
| B05 | Invalid HTML: Code before @page directive | `CharacterDetails.razor:1-5` | ✅ FIXED | Helper method was incorrectly placed before `@page` directive - removed duplicate |
| B06 | Missing fallback for missing avatars | `DepartmentOverview.razor:88` | ✅ FIXED | Changed fallback from "default.png" to "adam1.png" which exists |
| B07 | ClosedXML commented out but remnants remain | `Program.cs:7`, `WeeklyTracker.razor:15` | ⚠️ COMMENTED | Excel library references commented but not cleaned up |
| B08 | Period parsing assumes specific format | Multiple | ⚠️ COMMENTED | `GetPeriodEndDate()` will fail silently on unexpected formats |

### Minor Issues

| ID | Issue | Location | Status | Description |
|----|-------|----------|--------|-------------|
| B09 | Unused imports | `_Imports.razor` | - | Some imports may not be needed |
| B10 | Console.WriteLine statements | Multiple | - | Debug logging left in production code |
| B11 | Magic numbers | `app.js:111` | ⚠️ COMMENTED | Slide width hardcoded as `180 + 16` |
| B12 | Terms modal shows loading spinner briefly | `MainLayout.razor` | ⚠️ COMMENTED | Flash of loading state before terms check completes |
| B13 | NavMenu references unused routes | `NavMenu.razor` | ⚠️ COMMENTED | Links to `/counter` and `/fetchdata` which are demo pages |

### Dropdown Fix (January 2026)
Fixed issue where WeeklyTracker department and period dropdowns were empty:
- Added null safety checks in `OnInitializedAsync()`
- Fixed period generation cutoff logic (was too restrictive)
- Added fallback to ensure at least current month periods are always available
- Added forced `StateHasChanged()` call after data load

### Code Quality Issues

| ID | Issue | Location | Description |
|----|-------|----------|-------------|
| C01 | Large monolithic components | `WeeklyTracker.razor` (1173 lines), `CharacterDetails.razor` (856 lines) | Should be broken into smaller components |
| C02 | Mixed concerns in TeamMember model | `TeamMember.cs` | Business logic (scoring, title calculation) mixed with data model |
| C03 | Inconsistent naming | Various | Mix of snake_case and camelCase for private fields |
| C04 | Empty stub files | `CharacterCard.cs`, `MetricDisplay.cs`, etc. | Unused files should be removed or implemented |

---

## Deployment Notes

### Architecture

The app uses a split deployment model:
- **Frontend:** Blazor WebAssembly hosted on GitHub Pages
- **Backend API:** ASP.NET Core Web API hosted on Azure App Service
- **Database:** Azure SQL Server

In DEBUG mode, the frontend uses `MockDataService` (in-memory). In RELEASE mode, it uses `HttpDataService` to call the API.

### GitHub Pages Deployment (Frontend)

The app is configured for GitHub Pages with:
- SPA routing script in `index.html` (rafgraph/spa-github-pages pattern)
- `<base href="/">` in `index.html`
- `404.html` for fallback routing

### API Configuration

The API base URL is configured in `wwwroot/appsettings.json` → `ApiBaseUrl`. Program.cs uses this to create the `HttpClient`.

### Build Command
```bash
dotnet publish -c Release
```

### Post-Build Steps
1. Copy contents of `bin/Release/net8.0/publish/wwwroot` to GitHub Pages
2. Ensure `index.html` and `404.html` are at root
3. Verify `_framework` folder is included

### Known Deployment Issues
- Base href may need adjustment if not at domain root
- Tailwind CDN script requires internet connection
- First load downloads ~10-15MB of Blazor WASM resources

---

## Quick Reference: File Locations

| What | Where |
|------|-------|
| Add team members | `Services/MockDataService.cs` → `InitializeMockData()` |
| Add department metrics | `WeeklyTracker.razor` → `_departmentMetrics` dictionary |
| Add pod members | `Services/MockDataService.cs` → `InitializeMockData()` (use pod name as Department) |
| Add new pod | `PodIndex.razor`, `PodOverview.razor`, `PodTracker.razor` → `PodDepartments` HashSet |
| Modify pod KPI weights | `PodOverview.razor` & `PodTracker.razor` → `_metricWeights` dictionary |
| Modify pod KPI targets | `PodOverview.razor` & `PodTracker.razor` → `_metricTargets` dictionary |
| Change admin password | `WeeklyTracker.razor` & `PodTracker.razor` → `CorrectPassword` constant |
| Modify performance goals | Search for `GetMetricGoal` (exists in 3+ files) |
| Change XP formula | `TeamMember.cs` → `CurrentLevel` property |
| Add metric display names | `TeamMember.cs` → `GetMetricDisplayName()` |
| Modify radar chart metrics | `CharacterDetails.razor` → `GenerateRadarChartData()` |
| Edit CSS styles | `wwwroot/css/app.css` |
| JavaScript helpers | `wwwroot/js/app.js` |

---

## Contact

**Creator:** Drew Carrillo  
**Repository:** Private - Internal Use Only  
**Copyright:** © 2024-2026 Drew Carrillo. All Rights Reserved.


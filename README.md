# Team Performance Arena - Game-Inspired Scoreboard

A gamified performance visualization dashboard that presents team metrics in an engaging, video game-inspired interface.

![Team Performance Arena Screenshot](./screenshot.png)

## Project Overview

Team Performance Arena transforms traditional performance metrics into an interactive, game-inspired experience. The application visualizes team member performance as a character selection screen, similar to those found in video games, providing an engaging way to track, compare, and celebrate team achievements.

## Features

### 1. Character Select Screen
- Interactive carousel of team members styled as game characters
- Each card displays:
  - Team member name
  - Performance-based title (automatically assigned)
  - Strongest performance metric with visual indicators
  - Visual distinction for top performers (gold star)

### 2. Department Dashboard
- Department-wide performance summary
- Overall performance score calculation
- Best and lowest performers in each metric category
- Visual progress bars for key metrics
- Color-coded performance indicators

### 3. Detailed Character View
- Full performance breakdown for each team member
- Interactive radar chart showing all metrics at once
- Performance comparison against team averages
- Color-coded indicators for top (gold) and bottom (red) performers
- Weighted metric display based on team performance

### 4. Weekly Performance Tracker
- Save snapshots of team performance by week
- Historical performance data persistence (using local storage)
- Week-over-week performance comparison
- Visual trend indicators for improving or declining metrics
- Data exporting capabilities

## Technology Stack

- **Frontend Framework**: Blazor WebAssembly (.NET 8)
- **UI Framework**: Tailwind CSS via CDN
- **Data Visualization**: Custom SVG-based charts
- **Data Storage**: Browser LocalStorage for persistence
- **Styling**: Custom CSS with Elden Ring-inspired dark theme

## Project Structure

```
GameScoreboard/
│
├── GameScoreboard.csproj       # Project file
├── Program.cs                  # Application entry point
├── _Imports.razor              # Razor component imports
├── App.razor                   # Main application component
├── wwwroot/                    # Static web assets
│   ├── css/
│   │   ├── app.css             # Application styles
│   │   └── tailwind-output.css # Compiled Tailwind CSS
│   ├── js/
│   │   └── app.js              # JavaScript interop
│   └── index.html              # Host page
├── Pages/
│   ├── Index.razor             # Overview screen (@ route="/")
│   ├── CharacterDetails.razor  # Character detail screen (@ route="/character/{Id:int}")
│   └── WeeklyTracker.razor     # Weekly tracking page
├── Shared/
│   ├── MainLayout.razor        # Main layout component
│   ├── RadarChart.razor        # Radar chart component
│   └── DepartmentSummaryCard.razor # Department summary component
├── Models/
│   ├── TeamMember.cs           # Team member data model
│   └── DepartmentSummary.cs    # Department summary model
└── Services/
    ├── DataService.cs          # Data service interface
    └── MockDataService.cs      # Mock data implementation
```

## Setup and Installation

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 or later (recommended) or Visual Studio Code with C# extensions

### Installation Steps

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/team-performance-arena.git
   cd team-performance-arena
   ```

2. Open the project in Visual Studio 2022 (or your preferred IDE)

3. Build the solution:
   ```
   dotnet build
   ```

4. Run the application:
   ```
   dotnet run
   ```

5. Navigate to `https://localhost:5001` in your browser

### Using Tailwind CSS

The project uses Tailwind CSS via CDN for simplicity. If you prefer to use a local installation:

1. Install Node.js and npm
2. Install Tailwind CSS:
   ```
   npm install -D tailwindcss
   npx tailwindcss init
   ```
3. Update the tailwind.config.js file
4. Create an input.css file in the wwwroot/css folder
5. Build Tailwind CSS:
   ```
   npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/tailwind-output.css
   ```

## Performance Metrics

The application tracks six key performance metrics:

1. **M365 Attach**: Microsoft 365 attachment rate (%)
2. **GSP (Warranty)**: Guarantee Service Plan attachment (%)
3. **Revenue**: Total sales revenue ($)
4. **ASP (Average Selling Price)**: Average price per transaction ($)
5. **Basket**: Average number of items per transaction
6. **PM Attach %**: Protection and Maintenance attachment rate (%)

Each metric has an associated title for top performers:
- **M365 Attach**: "Cloud Champion"
- **GSP (Warranty)**: "Guardian of Guarantees"
- **Revenue**: "Revenue Raider"
- **ASP (Average Selling Price)**: "Value Virtuoso"
- **Basket**: "Master Merchant"
- **PM Attach %**: "Productivity Pro"

## Key Components

### Character Cards
Interactive cards that display team member performance with game-inspired visuals. Each card shows:
- Team member name
- Performance-based title
- Strongest metric relative to team average
- Visual indicator for top performers

### Radar Chart
A custom SVG-based radar chart that visualizes all six performance metrics simultaneously, allowing for instant visual assessment of strengths and weaknesses.

### Department Summary
A comprehensive dashboard that shows team-wide performance statistics, including:
- Overall performance score
- Average metrics across the team
- Best and lowest performers for each category
- Visual progress indicators

### Weekly Tracker
A historical data tracking system that allows for:
- Saving weekly performance snapshots
- Comparing current performance to historical data
- Identifying trends and improvements over time
- Exporting performance data

## Customization

### Modifying Metrics
To add or change metrics, update the following files:
- `TeamMember.cs`: Update the `MetricDisplayNames` and `MetricTitles` dictionaries
- `DepartmentSummary.cs`: Update the `NormalizeMetricValue` method with new max values
- `CharacterDetails.razor`: Update display logic for new metrics

### Styling Customization
The application uses an Elden Ring-inspired dark theme with gold accents. To customize:
- Modify `app.css` for global styles
- Update the Tailwind classes in component files
- Adjust color schemes in individual components

### Adding New Features
The modular architecture allows for easy extension. Common additions include:
- Authentication system for secure access
- Backend API integration for real-time data
- Leaderboard competitions
- Achievement badges and rewards system

## Using the Application

### Viewing Team Performance
1. The home page displays a carousel of all team members
2. Scroll through cards using arrow buttons or drag/swipe
3. The department summary at the top shows overall team performance
4. Click on any character card to view detailed metrics

### Exploring Detailed Metrics
1. The character details page shows all metrics for a selected team member
2. Metrics are sorted by relative strength compared to team averages
3. The radar chart provides a visual representation of all metrics
4. Color-coded indicators show where they excel or need improvement

### Tracking Weekly Performance
1. Click "Weekly Tracker" to access the historical data page
2. Set a date and click "Save Week's Data" to store current metrics
3. View historical data in chronological order
4. Click "Compare" to see detailed week-over-week changes

## Future Enhancements

- **Backend Integration**: Connect to real-time data sources
- **Export Capabilities**: Generate PDF or Excel reports
- **Team Competitions**: Add team vs. team comparison features
- **Achievement System**: Implement badges and milestones
- **Mobile App**: Create a companion mobile application
- **AI Insights**: Add performance trend predictions

## Credits

- Design inspired by Elden Ring character selection screens
- Developed using Blazor WebAssembly and Tailwind CSS
- Custom SVG visualizations for performance metrics

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## How to Use

This application displays team performance metrics in a visually appealing way. Team members are shown as "champions" with their strongest metrics highlighted.

### Features

- View team members as game characters with their strongest metrics
- See department-wide performance summary
- Compare individual performance against team averages
- Hidden "Weekly Tracker" feature accessible by pressing the period (.) key three times

## Deployment

This app is deployed using GitHub Pages and can be accessed at: https://drew-834.github.io/TeamArena/

## Development

To run locally:

```
dotnet restore
dotnet run
```

## Technologies Used

- Blazor WebAssembly
- Tailwind CSS
- GitHub Pages for hosting

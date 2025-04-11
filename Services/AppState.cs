using System;
using System.Threading.Tasks;

namespace GameScoreboard.Services
{
    /// <summary>
    /// Manages application-wide state and notifications.
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// Event triggered when metrics have been updated (e.g., saved in WeeklyTracker).
        /// Components can subscribe to this to refresh their data.
        /// </summary>
        public event Func<Task>? OnMetricsUpdatedAsync;

        /// <summary>
        /// Notifies subscribers that metrics have been updated.
        /// </summary>
        public async Task NotifyMetricsUpdatedAsync()
        {
            if (OnMetricsUpdatedAsync != null)
            {
                // Invoke the event asynchronously
                await OnMetricsUpdatedAsync.Invoke();
            }
        }
    }
} 
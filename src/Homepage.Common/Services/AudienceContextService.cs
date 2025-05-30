using Serilog;

using System.Collections.Generic;

namespace Homepage.Common.Services
{
    public class AudienceContextService
    {
        private string _currentAudience = "developer"; // Default audience
        public string CurrentAudience
        {
            get => _currentAudience;
            private set
            {
                if (_currentAudience != value)
                {
                    _currentAudience = value;
                    Log.Logger.ForContext<AudienceContextService>().Information("Audience changed to: {Audience}", _currentAudience);
                    OnAudienceChanged?.Invoke(); // Notify subscribers
                }
            }
        }

        // Event to notify components when the audience changes
        public event Action? OnAudienceChanged;

        public AudienceContextService()
        {
            Log.Logger.ForContext<AudienceContextService>().Information("AudienceContextService initialized with default audience: {Audience}", _currentAudience);
        }

        public void SetAudience(string audience)
        {
            // Normalize the audience string (e.g., to lowercase) for consistent comparison
            string normalizedAudience = audience.ToLowerInvariant();

            // Validate against a known list of audiences if desired,
            // or allow any string for flexibility. For now, we'll allow any.
            // You might want to define an enum for AudienceType later for strictness.
            CurrentAudience = normalizedAudience;
        }

        // Optional: Method to get a list of available audiences (could be hardcoded or derived from metadata)
        public List<string> GetAvailableAudiences()
        {
            // For now, hardcode the audiences. In a more advanced scenario,
            // you might derive these from the distinct TargetAudiences in your ContentMetadata.
            return new List<string> {
                "developer",
                "techlead",
                "recruiter",
                "all" // An "all" audience to view everything
            };
        }
    }
}

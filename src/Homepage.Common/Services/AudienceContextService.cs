using Microsoft.AspNetCore.Components;

using MudBlazor;

using Serilog;

using System.Collections.Generic;

namespace Homepage.Common.Services
{
    public class AudienceContextService
    {
        private string _currentAudience = "developer";
        public string CurrentAudience
        {
            get => _currentAudience;
            private set
            {
                if (_currentAudience != value)
                {
                    _currentAudience = value;
                    Log.Logger.ForContext<AudienceContextService>().Information("Audience changed to: {Audience}", _currentAudience);
                    OnAudienceChanged?.Invoke();
                }
            }
        }

        public event Action? OnAudienceChanged;

        public AudienceContextService()
        {
            Log.Logger.ForContext<AudienceContextService>().Information("AudienceContextService initialized with default audience: {Audience}", _currentAudience);
        }

        public void SetAudience(string audience)
        {
            string normalizedAudience = audience.ToLowerInvariant();
            var availableAudiences = GetAvailableAudiences();
            if (!availableAudiences.Contains(normalizedAudience))
            {
                Log.Logger.ForContext<AudienceContextService>().Warning("Attempted to set invalid audience: {Audience}. Available audiences: {AvailableAudiences}", normalizedAudience, availableAudiences);
            }

            CurrentAudience = normalizedAudience;
            Log.Logger.ForContext<AudienceContextService>().Information("Setting audience to: {Audience}", normalizedAudience);
        }

        public List<string> GetAvailableAudiences()
        {
            return new List<string> {
                "developer",
                "techlead",
                "recruiter",
                "all"
            };
        }
    }
}

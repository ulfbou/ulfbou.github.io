using Microsoft.JSInterop;

using Serilog;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Service for tracking scroll position and notifying Blazor components of active sections.
    /// </summary>
    public class ScrollTrackerService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<ScrollTrackerService>? _objectRef;
        private readonly ILogger _logger;

        public event Action<string>? OnSectionChanged;

        public ScrollTrackerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _logger = Log.Logger.ForContext<ScrollTrackerService>();
        }

        /// <summary>
        /// Starts observing headings in the DOM for scroll position changes.
        /// </summary>
        public async Task StartObservingAsync()
        {
            _objectRef = DotNetObjectReference.Create(this);
            try
            {
                await _jsRuntime.InvokeVoidAsync("scrollTracker.observeHeadings", _objectRef);
                _logger.Debug("ScrollTrackerService started observing headings.");
            }
            catch (JSException ex)
            {
                _logger.Error(ex, "Failed to start JavaScript scroll observer. Ensure 'scrollTracker.observeHeadings' is defined and DOM is ready.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unexpected error occurred while starting scroll observer.");
            }
        }

        /// <summary>
        /// Invoked from JavaScript when a new heading becomes the active section.
        /// </summary>
        /// <param name="sectionId">The ID of the currently active heading.</param>
        [JSInvokable]
        public void UpdateActiveSection(string sectionId)
        {
            _logger.Debug("Active section updated to: {SectionId}", sectionId);
            OnSectionChanged?.Invoke(sectionId);
        }

        /// <summary>
        /// Disposes the .NET object reference to prevent memory leaks.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_objectRef != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("scrollTracker.dispose");
                    _objectRef.Dispose();
                    _objectRef = null;
                    _logger.Debug("ScrollTrackerService disposed.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during ScrollTrackerService disposal.");
                }
            }
        }
    }
}

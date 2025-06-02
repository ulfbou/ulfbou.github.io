using Microsoft.JSInterop;

using Serilog;

namespace Homepage.Common.Services
{
    /// <summary>
    /// Service for tracking and reporting reading progress based on scroll position.
    /// </summary>
    public class ReadingProgressService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<ReadingProgressService>? _objectRef;
        private readonly ILogger _logger;

        public event Action<double>? OnScrollProgressChanged;

        public ReadingProgressService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _logger = Log.Logger.ForContext<ReadingProgressService>();
        }

        /// <summary>
        /// Initializes the JavaScript-side scroll progress tracking.
        /// </summary>
        public async Task InitAsync()
        {
            _objectRef = DotNetObjectReference.Create(this);
            try
            {
                await _jsRuntime.InvokeVoidAsync("readingProgressTracker.init", _objectRef);
                _logger.Information("ReadingProgressService initialized JavaScript tracker.");
            }
            catch (JSException ex)
            {
                _logger.Error(ex, "Failed to initialize JavaScript reading progress tracker. Ensure 'readingProgressTracker.init' is defined.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unexpected error occurred while initializing reading progress tracker.");
            }
        }

        /// <summary>
        /// Invoked from JavaScript to update the scroll progress.
        /// </summary>
        /// <param name="progress">The scroll progress as a percentage (0-100).</param>
        [JSInvokable]
        public void UpdateScrollProgress(double progress)
        {
            _logger.Debug("Scroll progress: {Progress}%", progress);
            OnScrollProgressChanged?.Invoke(progress);
        }

        /// <summary>
        /// Disposes the .NET object reference and cleans up JavaScript resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_objectRef != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("readingProgressTracker.dispose");
                    _objectRef.Dispose();
                    _objectRef = null;
                    _logger.Information("ReadingProgressService disposed.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during ReadingProgressService disposal.");
                }
            }
        }
    }
}

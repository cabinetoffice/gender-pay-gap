using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;

namespace GenderPayGap.WebUI.Search {

    public class SearchCacheUpdaterService : IHostedService, IDisposable
    {
        private Timer timer;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            CustomLogger.Information("Starting timer (SearchRepository.StartCacheUpdateThread)");

            if (!Global.DisableSearchCache)
            {
                timer = new Timer(
                    DoWork,
                    null,
                    dueTime: TimeSpan.FromSeconds(0), // How long to wait before the cache is first updated 
                    period: TimeSpan.FromMinutes(1));  // How often is the cache updated 
            }

            CustomLogger.Information("Started timer (SearchRepository.StartCacheUpdateThread)");

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            CustomLogger.Information("Starting cache update (SearchRepository.StartCacheUpdateThread)");

            try
            {
                SearchRepository.LoadSearchDataIntoCache();
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Error during cache update (SearchRepository.StartCacheUpdateThread): {ex.Message} {ex.StackTrace}", ex);
            }

            CustomLogger.Information("Finished cache update (SearchRepository.StartCacheUpdateThread)");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            CustomLogger.Information("Timed Hosted Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}

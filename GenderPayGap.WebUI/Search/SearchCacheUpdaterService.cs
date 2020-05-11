using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Search.CachedObjects;
using Microsoft.Extensions.Hosting;

namespace GenderPayGap.WebUI.Search {

    public class SearchCacheUpdaterService : IHostedService, IDisposable
    {
        private Timer timer;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            CustomLogger.Information("Starting timer (SearchRepository.StartCacheUpdateThread)");

            timer = new Timer(
                DoWork,
                null,
                dueTime: TimeSpan.FromSeconds(10), // How long to wait before the cache is first updated 
                period: TimeSpan.FromMinutes(1));  // How often is the cache updated 

            CustomLogger.Information("Started timer (SearchRepository.StartCacheUpdateThread)");

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            CustomLogger.Information("Starting cache update (SearchRepository.StartCacheUpdateThread)");

            var dataRepository = MvcApplication.ContainerIoC.Resolve<IDataRepository>();
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.LoadAllOrganisations(dataRepository);
            List<SearchCachedUser> allUsers = SearchRepository.LoadAllUsers(dataRepository);

            SearchRepository.cachedOrganisations = allOrganisations;
            SearchRepository.cachedUsers = allUsers;
            SearchRepository.cacheLastUpdated = VirtualDateTime.Now;

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

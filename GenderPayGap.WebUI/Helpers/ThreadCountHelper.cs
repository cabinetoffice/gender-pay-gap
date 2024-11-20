using System.Threading;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ThreadCountHelper
    {

        public static string GetThreadCount()
        {
            ThreadPool.GetMinThreads(out int workerMin, out int ioMin);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);
            ThreadPool.GetAvailableThreads(out int workerFree, out int ioFree);
            return
                $"Threads (Worker busy:{workerMax - workerFree:N0} min:{workerMin:N0} max:{workerMax:N0}, I/O busy:{ioMax - ioFree:N0} min:{ioMin:N0} max:{ioMax:N0})";
        }

        public static string SetThreadCount()
        {
            var ioMin = Global.MinIOThreads;
            var workerMin = Global.MinWorkerThreads;
            ThreadPool.SetMinThreads(workerMin, ioMin);
            return $"Min Threads Set (Work:{workerMin:N0}, I/O: {ioMin:N0})";
        }

    }
}

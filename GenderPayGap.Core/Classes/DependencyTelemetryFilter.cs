using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace GenderPayGap.Core.Classes
{
    public class DependencyTelemetryFilter : ITelemetryProcessor
    {

        private readonly string HostName;
        private readonly ITelemetryProcessor Next;
        private readonly string OperationName;
        private readonly string[] ResultCodes;

        /// <summary>
        ///     Initialise Telementry filter
        /// </summary>
        /// <param name="next">The next processor to execute</param>
        /// <param name="hostName">The suffix of the host name of the dependency (e.g., file.core.windows.net)</param>
        /// <param name="operationName">The HTTP method and path prefix (e.g., GET /Common/App_Data/ </param>
        /// <param name="resultCodes">The list of result codes to regard as successful</param>
        public DependencyTelemetryFilter(ITelemetryProcessor next, string hostName, string operationName, params string[] resultCodes)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            if (resultCodes == null || resultCodes.Length == 0 || resultCodes.Any(on => string.IsNullOrWhiteSpace(on)))
            {
                throw new ArgumentNullException(nameof(resultCodes));
            }

            HostName = hostName;
            OperationName = operationName;
            ResultCodes = resultCodes;
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;

            if (!string.IsNullOrWhiteSpace(dependency?.Context?.Operation?.Name)
                && dependency.Target.EndsWith(HostName, StringComparison.CurrentCultureIgnoreCase)
                && dependency.Name.StartsWith(OperationName, StringComparison.CurrentCultureIgnoreCase)
                && ResultCodes.Any(rc => rc.Equals(dependency.ResultCode, StringComparison.OrdinalIgnoreCase)))
            {
                dependency.Success = true;
                Next.Process(dependency);
            }
            else
            {
                Next.Process(item);
            }
        }

    }
}

using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ExceptionDetailsHelper
    {

        private const string LogName = "GenderPayGap";


        public static string GetDetailsText(Exception ex)
        {
            return JsonConvert.SerializeObject(ex.GetDetails(), Formatting.Indented);
        }

        private static object GetDetails(this Exception ex)
        {
            if (ex == null)
            {
                return null;
            }

            string message = ex.Message;

            if (ex is AggregateException)
            {
                var aex = (AggregateException) ex;

                var c = 0;
                foreach (Exception innerEx in aex.InnerExceptions)
                {
                    message += ++c
                               + " of "
                               + aex.InnerExceptions.Count
                               + ":"
                               + JsonConvert.SerializeObject(GetDetails(innerEx))
                               + Environment.NewLine;
                }
            }

            if (ex.InnerException != null)
            {
                return new ErrorDetails {
                    Message = message,
                    Source = ex.Source,
                    Type = ex.GetType(),
                    StackTrace = ex.FullStackTrace(),
                    InnerException = ex.InnerException.GetDetails()
                };
            }

            return new ErrorDetails {Message = message, Source = ex.Source, Type = ex.GetType(), StackTrace = ex.FullStackTrace()};
        }


        /// <summary>
        ///     Provides full stack trace for the exception that occurred.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        /// <param name="environmentStackTrace">Environment stack trace, for pulling additional stack frames.</param>
        private static string FullStackTrace(this Exception exception)
        {
            List<string> environmentStackTraceLines = GetUserStackTraceLines(Environment.StackTrace);
            if (environmentStackTraceLines.Count > 0)
            {
                environmentStackTraceLines.RemoveAt(0);
            }

            if (exception != null)
            {
                List<string> stackTraceLines = GetStackTraceLines(exception.StackTrace);
                environmentStackTraceLines.AddRange(stackTraceLines);
            }

            return exception.Message + Environment.NewLine + string.Join(Environment.NewLine, environmentStackTraceLines);
        }

        /// <summary>
        ///     Gets a list of stack frame lines, as strings.
        /// </summary>
        /// <param name="stackTrace">Stack trace string.</param>
        private static List<string> GetStackTraceLines(string stackTrace)
        {
            return stackTrace.Split(Environment.NewLine).ToList();
        }

        /// <summary>
        ///     Gets a list of stack frame lines, as strings, only including those for which line number is known.
        /// </summary>
        /// <param name="fullStackTrace">Full stack trace, including external code.</param>
        private static List<string> GetUserStackTraceLines(string fullStackTrace)
        {
            var outputList = new List<string>();
            var regex = new Regex(@"([^\)]*\)) in (.*):line (\d)*$");

            List<string> stackTraceLines = GetStackTraceLines(fullStackTrace);
            foreach (string stackTraceLine in stackTraceLines)
            {
                if (regex.IsMatch(stackTraceLine))
                {
                    outputList.Add(stackTraceLine);
                }
            }

            return outputList;
        }

        private class ErrorDetails
        {

            public string Message { get; set; }
            public string Source { get; set; }
            public Type Type { get; set; }
            public string StackTrace { get; set; }
            public object InnerException { get; set; }

        }

    }
}

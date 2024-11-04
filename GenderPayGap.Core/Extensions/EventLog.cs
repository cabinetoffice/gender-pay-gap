using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GenderPayGap.Extensions
{
    public static class EventLog
    {

        public const string LogName = "GenderPayGap";

        private static Assembly _TopAssembly;
        public static string LogSource;

        static EventLog()
        {
            LogSource = TopAssembly.GetName().Name;
        }

        public static Assembly TopAssembly
        {
            get
            {
                if (_TopAssembly == null)
                {
                    if (Assembly.GetEntryAssembly() != null)
                    {
                        _TopAssembly = Assembly.GetEntryAssembly();
                    }
                    //else if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null)
                    //{
                    //    _TopAssembly = HttpContext.Current.ApplicationInstance.GetType().Assembly;
                    //}
                    else
                    {
                        var stackTrace = new StackTrace(); // get call stack
                        StackFrame[] stackFrames = stackTrace.GetFrames();

                        // write call stack method names
                        for (int i = stackFrames.Length - 1; i > -1; i--)
                        {
                            _TopAssembly = stackFrames[i].GetMethod().ReflectedType.Assembly;
                            if (_TopAssembly.GetName() != null && _TopAssembly.GetName().Name.ContainsI(LogName))
                            {
                                break;
                            }

                            try
                            {
                                if (((AssemblyCompanyAttribute) Attribute.GetCustomAttribute(
                                    _TopAssembly,
                                    typeof(AssemblyCompanyAttribute),
                                    false)).Company.ContainsI(LogName))
                                {
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                }

                return _TopAssembly;
            }
        }


        public static string GetDetailsText(this Exception ex)
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
        public static string FullStackTrace(this Exception exception)
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

        public class ErrorDetails
        {

            public string Message { get; set; }
            public string Source { get; set; }
            public Type Type { get; set; }
            public string StackTrace { get; set; }
            public object InnerException { get; set; }

        }

    }
}

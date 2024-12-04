using Newtonsoft.Json;
using Serilog;

namespace GenderPayGap.Core.Classes.Logger
{
    public static class CustomLogger
    {

        public static void Debug(string message, object values = null)
        {
            values = SanitisedValues(values);
            Log.Debug(GetLogMessage(message, values), values);
        }

        public static void Information(string message, object values = null)
        {
            values = SanitisedValues(values);
            Log.Information(GetLogMessage(message, values), values);
        }

        public static void Warning(string message, object values = null)
        {
            values = SanitisedValues(values);
            Log.Warning(GetLogMessage(message, values), values);
        }

        public static void Error(string message, object values = null)
        {
            values = SanitisedValues(values);
            Log.Error(GetLogMessage(message, values), values);
        }

        public static void Fatal(string message, object values = null)
        {
            values = SanitisedValues(values);
            Log.Fatal(GetLogMessage(message, values), values);
        }

        private static object SanitisedValues(object values)
        {
            if (values is Exception exception)
            {
                values = new
                {
                    exception.Message,
                    exception.StackTrace,
                };
            }

            try
            {
                // The logger doesn't use JsonConvert but it is doing something similar
                // We try to convert values to JSON to see if it will throw an exception
                JsonConvert.SerializeObject(values);
            }
            catch (JsonSerializationException ex)
            {
                Error("SERILOG ERROR: Can't serialize values " + ex.Path);
                throw;
            }

            return values;
        }

        private static string GetLogMessage(string message, object values = null)
        {
            return values == null ? message : message + ". Log: {@Values}";
        }

    }
}

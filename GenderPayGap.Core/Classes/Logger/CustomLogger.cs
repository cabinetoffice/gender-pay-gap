using Newtonsoft.Json;
using Serilog;

namespace GenderPayGap.Core.Classes.Logger
{
    public static class CustomLogger
    {

        public static void Debug(string message, object values = null)
        {
            Log.Debug(GetLogMessage(message, values), values);
        }

        public static void Information(string message, object values = null)
        {
            Log.Information(GetLogMessage(message, values), values);
        }

        public static void Warning(string message, object values = null)
        {
            Log.Warning(GetLogMessage(message, values), values);
        }

        public static void Error(string message, object values = null)
        {
            Log.Error(GetLogMessage(message, values), values);
        }

        public static void Fatal(string message, object values = null)
        {
            Log.Fatal(GetLogMessage(message, values), values);
        }

        private static string GetLogMessage(string message, object values = null)
        {
            try
            {
                JsonConvert.SerializeObject(values);
                return values == null ? message : message + ". Log: {@Values}";
            }
            catch (JsonSerializationException ex)
            {
                Error("SERILOG ERROR: Can't serialize values");
                throw;
            }
        }

    }
}

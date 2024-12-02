using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace GenderPayGap.WebUI.Helpers
{
    public static class CsvWriter
    {

        /*
         * The default injection characters are: '=', '@', '+', '-'
         * We ignore '-' because we don't want the negative figures to be sanitized (i.e., prepended with a tab character)
         * We sanitize manually the other fields that start with '-' using the CustomConverter
         */
        private static readonly char[] InjectionCharacters = {'=', '@', '+'};

        public static T Write<T>(Func<MemoryStream, StreamReader, StreamWriter, CsvHelper.CsvWriter, T> write)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                InjectionCharacters = InjectionCharacters,
                InjectionOptions = InjectionOptions.Escape,
                TrimOptions = TrimOptions.InsideQuotes,
                ShouldQuote = (_) => true
            };
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, config))
            {
                csvWriter.Context.TypeConverterCache.AddConverter<string>(new CustomConverter());
                return write(memoryStream, streamReader, streamWriter, csvWriter);
            }
        }

        private class CustomConverter : DefaultTypeConverter
        {
        
            public override string ConvertToString(object value,
                IWriterRow row,
                MemberMapData memberMapData)
            {
                if (HasToBeSanitized(value))
                {
                    return $"'{value}";
                }
        
                return base.ConvertToString(value, row, memberMapData);
            }
        
            private bool HasToBeSanitized(object value)
            {
                return value is string s && !decimal.TryParse(s, out _) && s.StartsWith('-');
            }
        
        }

    }
}

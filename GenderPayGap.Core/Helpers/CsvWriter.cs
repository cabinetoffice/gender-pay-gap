using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace GenderPayGap.Core.Helpers
{
    public static class CsvWriter
    {

        public static T Write<T>(Func<MemoryStream, StreamReader, StreamWriter, CsvHelper.CsvWriter, T> write)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                InjectionCharacters = InjectionCharacters, SanitizeForInjection = true
            };
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, config))
            {
                csvWriter.Configuration.TypeConverterCache.AddConverter<string>(new CustomConverter());
                return write(memoryStream, streamReader, streamWriter, csvWriter);
            }
        }

        /*
         * The default injection characters are: '=', '@', '+', '-'
         * We ignore '-' because we don't want the negative figures to be sanitized (i.e., prepended with a tab character)
         * We sanitize manually the other fields that start with '-' using the CustomConverter
         */
        private static readonly char[] InjectionCharacters = {'=', '@', '+'};

        private class CustomConverter : DefaultTypeConverter
        {

            public override string ConvertToString(object value,
                IWriterRow row,
                MemberMapData memberMapData)
            {
                if (HasToBeSanitized(value))
                {
                    return '\t' + value.ToString();
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

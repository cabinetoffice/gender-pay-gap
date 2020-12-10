using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Models.AdminReferenceData {

    internal static class ReferenceDataHelper {

        public static bool TryParseCsvFileWithHeadings<TFile>(
            IFormFile file,
            IEnumerable<string> expectedHeadings,
            out List<TFile> rows,
            out string errorMessage)
        {
            rows = null;
            errorMessage = null;

            if (file == null)
            {
                errorMessage = "Select a CSV file";
                return false;
            }

            if (!Path.GetExtension(file.FileName).Equals(".csv"))
            {
                errorMessage = "The selected file must be a CSV";
                return false;
            }

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
                    {
                        IgnoreQuotes = false,
                        TrimOptions = TrimOptions.InsideQuotes,
                        MissingFieldFound = null,
                        SanitizeForInjection = true
                    };

                    using (var csvReader = new CsvReader(reader, csvConfiguration))
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();
                        string[] fieldHeaders = csvReader.Context.HeaderRecord;

                        if (!fieldHeaders.SequenceEqual(expectedHeadings))
                        {
                            errorMessage =
                                "The selected file has the wrong column headings. "
                                + "Download the current version of this data from this page to find the correct format.";
                            return false;
                        }

                        rows = csvReader.GetRecords<TFile>().ToList();

                        if (rows.Count == 0)
                        {
                            errorMessage = "The selected file is empty";
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "The selected file could not be uploaded - try again.";
                return false;
            }
        }

    }
}
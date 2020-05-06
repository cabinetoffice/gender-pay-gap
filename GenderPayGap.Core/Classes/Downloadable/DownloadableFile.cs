using System;
using GenderPayGap.Core.Interfaces.Downloadable;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes.Downloadable
{
    public class DownloadableFile : IDownloadableFile
    {

        public DownloadableFile(string filePath)
        {
            Filepath = filePath;
        }

        public string Filepath { get; }

        public string Name => Filepath.AfterLastAny("\\/", StringComparison.Ordinal);

        public DateTime? Modified { get; set; } = null;

        public string Type
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return null;
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Submission History";
                }

                return null;
            }
        }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return null;
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Submission History";
                }

                return null;
            }
        }

        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return null;
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Audit history of approved and rejected registrations.";
                }

                return null;
            }
        }

    }
}

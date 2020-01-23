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

                if (Name.Contains("ErrorLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Error logs";
                }

                if (Name.Contains("WarningLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Warning logs";
                }

                if (Name.Contains("InfoLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Information logs";
                }

                if (Name.Contains("BadSicLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Bad SIC Codes Log";
                }

                if (Name.Contains("ManualChangeLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Manual Changes Log";
                }

                if (Name.Contains("RegistrationLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Registration History";
                }

                if (Name.Contains("searchLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Search logs";
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Submission History";
                }

                if (Name.Contains("userLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "User Logs";
                }

                if (Name.Contains("emailSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Email Send Log";
                }

                if (Name.Contains("stannpSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Stannp Send Log";
                }

                if (Name.Contains("debugLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Debug Log from system";
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

                if (Name.Contains("ErrorLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Error log";
                }

                if (Name.Contains("WarningLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Warning log";
                }

                if (Name.Contains("InfoLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Information log";
                }

                if (Name.Contains("BadSicLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Bad SIC Codes Logs";
                }

                if (Name.Contains("ManualChangeLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Manual Changes Log";
                }

                if (Name.Contains("RegistrationLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Registration History";
                }

                if (Name.Contains("SearchLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Search log";
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Submission History";
                }

                if (Name.Contains("userLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "User Activity Log";
                }

                if (Name.Contains("emailSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Email Send Log";
                }

                if (Name.Contains("stannpSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Stannp Send Log";
                }

                if (Name.Contains("debugLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Debug Log from system";
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

                if (Name.Contains("ErrorLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Error log from system";
                }

                if (Name.Contains("WarningLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Warning log from system";
                }

                if (Name.Contains("InfoLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Information log from system";
                }

                if (Name.Contains("BadSicLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Companies and their unknown SIC Codes from Companies House.";
                }

                if (Name.Contains("ManualChangeLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Manual change history.";
                }

                if (Name.Contains("RegistrationLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Audit history of approved and rejected registrations.";
                }

                if (Name.Contains("SearchLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Searches carried out by users.";
                }

                if (Name.Contains("submissionLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Audit history of approved and rejected registrations.";
                }

                if (Name.Contains("userLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "A list of all user account activity.";
                }

                if (Name.Contains("emailSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Log of sent email messages via Gov Notify or SendGrid.";
                }

                if (Name.Contains("stannpSendLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Log of sent PIN Letters to Stannp.com.";
                }

                if (Name.Contains("debugLog", StringComparison.OrdinalIgnoreCase))
                {
                    return "Debug information";
                }

                return null;
            }
        }

    }
}

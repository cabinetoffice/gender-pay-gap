using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebJob
{
    public static class WebJobGlobal
    {

        public static int CertExpiresWarningDays => Config.GetAppSetting("CertExpiresWarningDays").ToInt32(30);
        public static string ExternalHost => Config.GetAppSetting("EXTERNAL_HOST");
        public static string AzureStorageConnectionString => Config.GetConnectionString("AzureStorage");
        public static List<string> GeoDistributionList => Config.GetAppSetting("GEODistributionList").Split(";", StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        public static string Culture => Config.GetAppSetting("Culture");
        public static string DatabaseConnectionString => Config.GetConnectionString("GpgDatabase");
        public static string DefaultEncryptionKey => Config.GetAppSetting("DefaultEncryptionKey");


    }
}

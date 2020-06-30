using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebJob
{
    public static class WebJobGlobal
    {

        public static string AzureStorageConnectionString => Config.GetConnectionString("AzureStorage");
        public static string Culture => Config.GetAppSetting("Culture");
        public static string DatabaseConnectionString => Config.GetConnectionString("GpgDatabase");
        public static string DefaultEncryptionKey => Config.GetAppSetting("DefaultEncryptionKey");


    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using GenderPayGap.Core;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext
    {

        public static void DeleteAllTestRecords(DateTime? deadline = null)
        {
            if (Config.IsProduction())
            {
                throw new Exception("Attempt to delete all test data from production environment");
            }

            if (string.IsNullOrWhiteSpace(Global.TestPrefix))
            {
                throw new ArgumentNullException(nameof(Global.TestPrefix));
            }

            if (deadline == null || deadline.Value == DateTime.MinValue)
            {
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{Global.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{Global.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{Global.TestPrefix}%'");
                ExecuteSqlCommand($"DELETE Users WITH (ROWLOCK) where Firstname like '{Global.TestPrefix}%'");
                ExecuteSqlCommand($"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{Global.TestPrefix}%'");
            }
            else
            {
                string dl = deadline.Value.ToString("yyyy-MM-dd HH:mm:ss");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{Global.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{Global.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{Global.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand($"DELETE Users WITH (ROWLOCK) where Firstname like '{Global.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand(
                    $"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{Global.TestPrefix}%' AND Created<'{dl}'");
            }
        }


        private static void ExecuteSqlCommand(string query, int timeOut = 120)
        {
            using (var sqlConnection1 = new SqlConnection(Config.GetConnectionString("GpgDatabase")))
            {
                using (var cmd = new SqlCommand(query, sqlConnection1))
                {
                    sqlConnection1.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = timeOut;
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}


using System;
using Microsoft.Data.SqlClient;

namespace ScholarPayments_PolesSU_Apps
{
    public class DatabaseConnection
    {
        private static string connectionString =
            "Server=localhost;" +
            "Database=ScholarshipPaym;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}

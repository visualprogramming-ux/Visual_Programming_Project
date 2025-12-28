using System.Data;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public static class DatabaseHelper
    {
        // Connection string - SQL Server Express
        private static readonly string ConnectionString = 
            "Server=FURQANARSHAD\\SQLEXPRESS;Database=RealEstateDB;Integrated Security=true;TrustServerCertificate=true;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


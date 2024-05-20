using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ServerLibrary.Data
{
    /// <summary>
    /// Class to initialize the Postgresql co
    /// </summary>
    public class NpgsqlDbContext
    {
            private static IConfiguration? _config;

            public static void Initialize(IConfiguration config)
            {
                _config = config;
            }

            public static NpgsqlConnection DBConnection()
            {
                string connString = _config["ConnectionStrings:ConnectionPG2"];
                NpgsqlConnection conn = new NpgsqlConnection(connString);
                return conn;
            }

        public static string GetConnectionString()
        {
            return  _config["ConnectionStrings:ConnectionPG2"];
        }
    }
}

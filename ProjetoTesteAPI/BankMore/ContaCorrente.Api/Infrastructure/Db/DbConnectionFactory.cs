using System.Data;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace ContaCorrente.Api.Infrastructure.Db
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _config;
        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Create()
        {
            var provider = _config.GetValue<string>("DbProvider") ?? "Sqlite";
            var connStr = _config.GetConnectionString("DefaultConnection") ?? "Data Source=contacorrente.db";
            return provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? new NpgsqlConnection(connStr)
                : new SqliteConnection(connStr);
        }
    }
}

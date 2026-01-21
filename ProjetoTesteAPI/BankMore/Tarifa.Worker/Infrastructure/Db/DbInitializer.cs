using Dapper;

namespace Tarifa.Worker.Infrastructure.Db
{
    public class DbInitializer
    {
        private readonly DbConnectionFactory _factory;
        private readonly IConfiguration _config;
        public DbInitializer(DbConnectionFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        public void Inicializar()
        {
            using var conn = _factory.Create();
            var provider = _config.GetValue<string>("DbProvider") ?? "Sqlite";
            var sqlTarifa = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS tarifas (id BIGSERIAL PRIMARY KEY, idcontacorrente BIGINT NOT NULL, datamovimento TIMESTAMP NOT NULL, valor NUMERIC(18,2) NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS tarifas (id INTEGER PRIMARY KEY AUTOINCREMENT, idcontacorrente INTEGER NOT NULL, datamovimento TEXT NOT NULL, valor REAL NOT NULL);";
            conn.Execute(sqlTarifa);
        }
    }
}

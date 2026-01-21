using Dapper;

namespace Transferencia.Api.Infrastructure.Db
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
            var sqlTrans = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS transferencia (id BIGSERIAL PRIMARY KEY, idcontacorrente_origem BIGINT NOT NULL, idcontacorrente_destino BIGINT NOT NULL, datamovimento TIMESTAMP NOT NULL, valor NUMERIC(18,2) NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS transferencia (id INTEGER PRIMARY KEY AUTOINCREMENT, idcontacorrente_origem INTEGER NOT NULL, idcontacorrente_destino INTEGER NOT NULL, datamovimento TEXT NOT NULL, valor REAL NOT NULL);";
            var sqlIdem = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS idempotencia (chave_idempotencia VARCHAR(100) PRIMARY KEY, requisicao TEXT NOT NULL, resultado TEXT NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS idempotencia (chave_idempotencia TEXT PRIMARY KEY, requisicao TEXT NOT NULL, resultado TEXT NOT NULL);";
            conn.Execute(sqlTrans);
            conn.Execute(sqlIdem);
        }
    }
}

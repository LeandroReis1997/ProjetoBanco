using Dapper;

namespace ContaCorrente.Api.Infrastructure.Db
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
            var sqlConta = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS contacorrente (id BIGSERIAL PRIMARY KEY, numero_conta VARCHAR(20) UNIQUE NOT NULL, nome VARCHAR(200) NOT NULL, ativo BOOLEAN NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS contacorrente (id INTEGER PRIMARY KEY AUTOINCREMENT, numero_conta TEXT UNIQUE NOT NULL, nome TEXT NOT NULL, ativo INTEGER NOT NULL);";
            var sqlMov = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS movimento (id BIGSERIAL PRIMARY KEY, idcontacorrente BIGINT NOT NULL, datamovimento TIMESTAMP NOT NULL, tipomovimento VARCHAR(1) NOT NULL, valor NUMERIC(18,2) NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS movimento (id INTEGER PRIMARY KEY AUTOINCREMENT, idcontacorrente INTEGER NOT NULL, datamovimento TEXT NOT NULL, tipomovimento TEXT NOT NULL, valor REAL NOT NULL);";
            var sqlIdem = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS idempotencia (chave_idempotencia VARCHAR(100) PRIMARY KEY, requisicao TEXT NOT NULL, resultado TEXT NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS idempotencia (chave_idempotencia TEXT PRIMARY KEY, requisicao TEXT NOT NULL, resultado TEXT NOT NULL);";
            conn.Execute(sqlConta);
            conn.Execute(sqlMov);
            conn.Execute(sqlIdem);
        }
    }
}

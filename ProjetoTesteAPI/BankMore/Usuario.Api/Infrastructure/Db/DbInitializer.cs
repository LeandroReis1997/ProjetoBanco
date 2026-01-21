using Dapper;

namespace Usuario.Api.Infrastructure.Db
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
            var sql = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                ? "CREATE TABLE IF NOT EXISTS usuario (id BIGSERIAL PRIMARY KEY, cpf VARCHAR(14) UNIQUE NOT NULL, numero_conta VARCHAR(20) UNIQUE NOT NULL, nome VARCHAR(200) NOT NULL, senha_hash VARCHAR(200) NOT NULL, salt VARCHAR(200) NOT NULL, ativo BOOLEAN NOT NULL);"
                : "CREATE TABLE IF NOT EXISTS usuario (id INTEGER PRIMARY KEY AUTOINCREMENT, cpf TEXT UNIQUE NOT NULL, numero_conta TEXT UNIQUE NOT NULL, nome TEXT NOT NULL, senha_hash TEXT NOT NULL, salt TEXT NOT NULL, ativo INTEGER NOT NULL);";
            conn.Execute(sql);
        }
    }
}

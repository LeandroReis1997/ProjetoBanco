using Dapper;
using ContaCorrente.Api.Infrastructure.Db;

namespace ContaCorrente.Api.Infrastructure.Repositories
{
    public class IdempotenciaRepository
    {
        private readonly DbConnectionFactory _factory;
        public IdempotenciaRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<string?> ObterResultado(string chave)
        {
            using var conn = _factory.Create();
            var sql = "SELECT resultado FROM idempotencia WHERE chave_idempotencia = @chave";
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { chave });
        }

        public async Task Salvar(string chave, string requisicao, string resultado)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) VALUES (@chave, @requisicao, @resultado)";
            await conn.ExecuteAsync(sql, new { chave, requisicao, resultado });
        }
    }
}

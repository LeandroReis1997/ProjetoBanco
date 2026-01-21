using Dapper;
using ContaCorrente.Api.Domain;
using ContaCorrente.Api.Infrastructure.Db;

namespace ContaCorrente.Api.Infrastructure.Repositories
{
    public class MovimentoRepository
    {
        private readonly DbConnectionFactory _factory;
        public MovimentoRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task Inserir(Movimento mov)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO movimento (idcontacorrente, datamovimento, tipomovimento, valor) VALUES (@ContaId, @DataMovimento, @TipoMovimento, @Valor)";
            await conn.ExecuteAsync(sql, mov);
        }

        public async Task<decimal> ObterSaldo(long contaId)
        {
            using var conn = _factory.Create();
            var sqlCredito = "SELECT COALESCE(SUM(valor),0) FROM movimento WHERE idcontacorrente = @id AND tipomovimento = 'C'";
            var sqlDebito = "SELECT COALESCE(SUM(valor),0) FROM movimento WHERE idcontacorrente = @id AND tipomovimento = 'D'";
            var creditos = await conn.ExecuteScalarAsync<decimal>(sqlCredito, new { id = contaId });
            var debitos = await conn.ExecuteScalarAsync<decimal>(sqlDebito, new { id = contaId });
            return creditos - debitos;
        }
    }
}

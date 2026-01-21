using Dapper;
using TransferenciaDomain = Transferencia.Api.Domain.Transferencia;
using Transferencia.Api.Infrastructure.Db;

namespace Transferencia.Api.Infrastructure.Repositories
{
    public class TransferenciaRepository
    {
        private readonly DbConnectionFactory _factory;
        public TransferenciaRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task Inserir(TransferenciaDomain transferencia)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO transferencia (idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor) VALUES (@ContaOrigemId, @ContaDestinoId, @DataMovimento, @Valor)";
            await conn.ExecuteAsync(sql, transferencia);
        }
    }
}

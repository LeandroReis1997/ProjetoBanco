using Dapper;
using Tarifa.Worker.Infrastructure.Db;

namespace Tarifa.Worker.Infrastructure.Repositories
{
    public class TarifaRepository
    {
        private readonly DbConnectionFactory _factory;
        public TarifaRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task Inserir(long contaId, decimal valor, DateTime dataHora)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO tarifas (idcontacorrente, datamovimento, valor) VALUES (@contaId, @dataHora, @valor)";
            await conn.ExecuteAsync(sql, new { contaId, dataHora, valor });
        }
    }
}

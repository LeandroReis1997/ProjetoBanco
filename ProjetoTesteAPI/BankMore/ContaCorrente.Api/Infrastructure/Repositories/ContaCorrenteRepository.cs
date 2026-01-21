using Dapper;
using ContaCorrenteDomain = ContaCorrente.Api.Domain.ContaCorrente;
using ContaCorrente.Api.Infrastructure.Db;

namespace ContaCorrente.Api.Infrastructure.Repositories
{
    public class ContaCorrenteRepository
    {
        private readonly DbConnectionFactory _factory;
        public ContaCorrenteRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<ContaCorrenteDomain?> ObterPorNumero(string numeroConta)
        {
            using var conn = _factory.Create();
            var sql = "SELECT id, numero_conta as NumeroConta, nome as Nome, ativo as Ativo FROM contacorrente WHERE numero_conta = @numero";
            return await conn.QueryFirstOrDefaultAsync<ContaCorrenteDomain>(sql, new { numero = numeroConta });
        }

        public async Task<ContaCorrenteDomain?> ObterPorId(long id)
        {
            using var conn = _factory.Create();
            var sql = "SELECT id, numero_conta as NumeroConta, nome as Nome, ativo as Ativo FROM contacorrente WHERE id = @id";
            return await conn.QueryFirstOrDefaultAsync<ContaCorrenteDomain>(sql, new { id });
        }

        public async Task<long> Criar(ContaCorrenteDomain conta)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO contacorrente (numero_conta, nome, ativo) VALUES (@NumeroConta, @Nome, @Ativo) RETURNING id";
            try
            {
                return await conn.ExecuteScalarAsync<long>(sql, conta);
            }
            catch
            {
                var sqliteSql = "INSERT INTO contacorrente (numero_conta, nome, ativo) VALUES (@NumeroConta, @Nome, @Ativo); SELECT last_insert_rowid();";
                return await conn.ExecuteScalarAsync<long>(sqliteSql, conta);
            }
        }

        public async Task AtualizarAtivo(long id, bool ativo)
        {
            using var conn = _factory.Create();
            var sql = "UPDATE contacorrente SET ativo = @ativo WHERE id = @id";
            await conn.ExecuteAsync(sql, new { id, ativo });
        }
    }
}

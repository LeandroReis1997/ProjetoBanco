using Dapper;
using UsuarioDomain = Usuario.Api.Domain.Usuario;
using Usuario.Api.Infrastructure.Db;

namespace Usuario.Api.Infrastructure.Repositories
{
    public class UsuarioRepository
    {
        private readonly DbConnectionFactory _factory;
        public UsuarioRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<UsuarioDomain?> ObterPorCpfOuNumero(string cpfOuNumero)
        {
            using var conn = _factory.Create();
            var sql = "SELECT id, cpf as Cpf, numero_conta as NumeroConta, nome as Nome, senha_hash as SenhaHash, salt as Salt, ativo as Ativo FROM usuario WHERE cpf = @valor OR numero_conta = @valor LIMIT 1";
            return await conn.QueryFirstOrDefaultAsync<UsuarioDomain>(sql, new { valor = cpfOuNumero });
        }

        public async Task<UsuarioDomain?> ObterPorId(long id)
        {
            using var conn = _factory.Create();
            var sql = "SELECT id, cpf as Cpf, numero_conta as NumeroConta, nome as Nome, senha_hash as SenhaHash, salt as Salt, ativo as Ativo FROM usuario WHERE id = @id";
            return await conn.QueryFirstOrDefaultAsync<UsuarioDomain>(sql, new { id });
        }

        public async Task<UsuarioDomain?> ObterPorNumero(string numeroConta)
        {
            using var conn = _factory.Create();
            var sql = "SELECT id, cpf as Cpf, numero_conta as NumeroConta, nome as Nome, senha_hash as SenhaHash, salt as Salt, ativo as Ativo FROM usuario WHERE numero_conta = @numero";
            return await conn.QueryFirstOrDefaultAsync<UsuarioDomain>(sql, new { numero = numeroConta });
        }

        public async Task<long> Criar(UsuarioDomain usuario)
        {
            using var conn = _factory.Create();
            var sql = "INSERT INTO usuario (cpf, numero_conta, nome, senha_hash, salt, ativo) VALUES (@Cpf, @NumeroConta, @Nome, @SenhaHash, @Salt, @Ativo) RETURNING id";
            try
            {
                return await conn.ExecuteScalarAsync<long>(sql, usuario);
            }
            catch
            {
                var sqliteSql = "INSERT INTO usuario (cpf, numero_conta, nome, senha_hash, salt, ativo) VALUES (@Cpf, @NumeroConta, @Nome, @SenhaHash, @Salt, @Ativo); SELECT last_insert_rowid();";
                return await conn.ExecuteScalarAsync<long>(sqliteSql, usuario);
            }
        }
    }
}

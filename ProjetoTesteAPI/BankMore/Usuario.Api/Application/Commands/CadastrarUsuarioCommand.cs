using System.Security.Cryptography;
using MediatR;
using UsuarioDomain = Usuario.Api.Domain.Usuario;
using Usuario.Api.Infrastructure.Repositories;
using Usuario.Api.Infrastructure.Security;

namespace Usuario.Api.Application.Commands
{
    public record CadastrarUsuarioCommand(string Cpf, string Senha, string Nome) : IRequest<CadastrarUsuarioResult>;

    public record CadastrarUsuarioResult(bool Sucesso, string TipoFalha, string Mensagem, string NumeroConta, long IdConta);

    public class CadastrarUsuarioHandler : IRequestHandler<CadastrarUsuarioCommand, CadastrarUsuarioResult>
    {
        private readonly UsuarioRepository _repo;
        private readonly PasswordHasher _hasher;
        public CadastrarUsuarioHandler(UsuarioRepository repo, PasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<CadastrarUsuarioResult> Handle(CadastrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!CpfValidator.Validar(request.Cpf))
            {
                return new CadastrarUsuarioResult(false, "INVALID_DOCUMENT", "CPF inválido", string.Empty, 0);
            }

            var existente = await _repo.ObterPorCpfOuNumero(request.Cpf);
            if (existente != null)
            {
                return new CadastrarUsuarioResult(false, "INVALID_DOCUMENT", "CPF já cadastrado", string.Empty, 0);
            }

            var numeroConta = await GerarNumeroConta();
            var salt = _hasher.GerarSalt();
            var hash = _hasher.Hash(request.Senha, salt);
            var nome = string.IsNullOrWhiteSpace(request.Nome) ? "Titular" : request.Nome.Trim();

            var usuario = new UsuarioDomain
            {
                Cpf = request.Cpf,
                NumeroConta = numeroConta,
                Nome = nome,
                SenhaHash = hash,
                Salt = salt,
                Ativo = true
            };

            var id = await _repo.Criar(usuario);
            return new CadastrarUsuarioResult(true, string.Empty, string.Empty, numeroConta, id);
        }

        private async Task<string> GerarNumeroConta()
        {
            var random = RandomNumberGenerator.GetInt32(10000000, 99999999);
            var numero = random.ToString();
            var existente = await _repo.ObterPorNumero(numero);
            return existente == null ? numero : await GerarNumeroConta();
        }
    }
}

using MediatR;
using Usuario.Api.Infrastructure.Repositories;
using Usuario.Api.Infrastructure.Security;

namespace Usuario.Api.Application.Commands
{
    public record ValidarSenhaCommand(long IdConta, string Senha) : IRequest<ValidarSenhaResult>;

    public record ValidarSenhaResult(bool Sucesso, string TipoFalha, string Mensagem);

    public class ValidarSenhaHandler : IRequestHandler<ValidarSenhaCommand, ValidarSenhaResult>
    {
        private readonly UsuarioRepository _repo;
        private readonly PasswordHasher _hasher;
        public ValidarSenhaHandler(UsuarioRepository repo, PasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<ValidarSenhaResult> Handle(ValidarSenhaCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _repo.ObterPorId(request.IdConta);
            if (usuario == null)
            {
                return new ValidarSenhaResult(false, "USER_UNAUTHORIZED", "Usuário não encontrado");
            }

            if (!_hasher.Verificar(request.Senha, usuario.Salt, usuario.SenhaHash))
            {
                return new ValidarSenhaResult(false, "USER_UNAUTHORIZED", "Senha inválida");
            }

            return new ValidarSenhaResult(true, string.Empty, string.Empty);
        }
    }
}

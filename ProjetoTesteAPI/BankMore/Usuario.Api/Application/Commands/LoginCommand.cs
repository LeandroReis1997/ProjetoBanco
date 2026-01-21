using MediatR;
using Usuario.Api.Infrastructure.Repositories;
using Usuario.Api.Infrastructure.Security;

namespace Usuario.Api.Application.Commands
{
    public record LoginCommand(string CpfOuNumero, string Senha) : IRequest<LoginResult>;

    public record LoginResult(bool Sucesso, string TipoFalha, string Mensagem, string Token, string NumeroConta, string Nome, long IdConta);

    public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly UsuarioRepository _repo;
        private readonly PasswordHasher _hasher;
        private readonly JwtTokenService _tokenService;
        public LoginHandler(UsuarioRepository repo, PasswordHasher hasher, JwtTokenService tokenService)
        {
            _repo = repo;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _repo.ObterPorCpfOuNumero(request.CpfOuNumero);
            if (usuario == null || !usuario.Ativo)
            {
                return new LoginResult(false, "USER_UNAUTHORIZED", "Usuário ou senha inválidos", string.Empty, string.Empty, string.Empty, 0);
            }

            if (!_hasher.Verificar(request.Senha, usuario.Salt, usuario.SenhaHash))
            {
                return new LoginResult(false, "USER_UNAUTHORIZED", "Usuário ou senha inválidos", string.Empty, string.Empty, string.Empty, 0);
            }

            var token = _tokenService.GerarToken(usuario.Id, usuario.NumeroConta, usuario.Nome);
            return new LoginResult(true, string.Empty, string.Empty, token, usuario.NumeroConta, usuario.Nome, usuario.Id);
        }
    }
}

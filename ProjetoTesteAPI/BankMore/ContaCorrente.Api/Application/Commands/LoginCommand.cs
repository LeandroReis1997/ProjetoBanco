using MediatR;
using ContaCorrente.Api.Infrastructure.Services;

namespace ContaCorrente.Api.Application.Commands
{
    public record LoginCommand(string CpfOuNumero, string Senha) : IRequest<LoginResult>;

    public record LoginResult(bool Sucesso, string TipoFalha, string Mensagem, string Token, string NumeroConta, string Nome);

    public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly UsuarioApiClient _usuario;
        public LoginHandler(UsuarioApiClient usuario)
        {
            _usuario = usuario;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var login = await _usuario.Login(request.CpfOuNumero, request.Senha);
            if (!login.Sucesso)
            {
                return new LoginResult(false, login.TipoFalha, login.Mensagem, string.Empty, string.Empty, string.Empty);
            }
            return new LoginResult(true, string.Empty, string.Empty, login.Token, login.NumeroConta, login.Nome);
        }
    }
}

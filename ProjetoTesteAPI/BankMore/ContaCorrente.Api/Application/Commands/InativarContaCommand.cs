using MediatR;
using ContaCorrente.Api.Infrastructure.Repositories;
using ContaCorrente.Api.Infrastructure.Services;

namespace ContaCorrente.Api.Application.Commands
{
    public record InativarContaCommand(string Token, string NumeroConta, string Senha) : IRequest<InativarContaResult>;

    public record InativarContaResult(bool Sucesso, string TipoFalha, string Mensagem);

    public class InativarContaHandler : IRequestHandler<InativarContaCommand, InativarContaResult>
    {
        private readonly ContaCorrenteRepository _repo;
        private readonly UsuarioApiClient _usuario;
        public InativarContaHandler(ContaCorrenteRepository repo, UsuarioApiClient usuario)
        {
            _repo = repo;
            _usuario = usuario;
        }

        public async Task<InativarContaResult> Handle(InativarContaCommand request, CancellationToken cancellationToken)
        {
            var conta = await _repo.ObterPorNumero(request.NumeroConta);
            if (conta == null)
            {
                return new InativarContaResult(false, "INVALID_ACCOUNT", "Conta não encontrada");
            }

            var valida = await _usuario.ValidarSenha(request.Token, request.Senha);
            if (!valida.Sucesso)
            {
                return new InativarContaResult(false, valida.TipoFalha, valida.Mensagem);
            }

            await _repo.AtualizarAtivo(conta.Id, false);
            return new InativarContaResult(true, string.Empty, string.Empty);
        }
    }
}

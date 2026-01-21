using MediatR;
using ContaCorrenteDomain = ContaCorrente.Api.Domain.ContaCorrente;
using ContaCorrente.Api.Infrastructure.Repositories;
using ContaCorrente.Api.Infrastructure.Services;

namespace ContaCorrente.Api.Application.Commands
{
    public record CadastrarContaCommand(string Cpf, string Senha, string Nome) : IRequest<CadastrarContaResult>;

    public record CadastrarContaResult(bool Sucesso, string TipoFalha, string Mensagem, string NumeroConta);

    public class CadastrarContaHandler : IRequestHandler<CadastrarContaCommand, CadastrarContaResult>
    {
        private readonly UsuarioApiClient _usuario;
        private readonly ContaCorrenteRepository _repo;
        public CadastrarContaHandler(UsuarioApiClient usuario, ContaCorrenteRepository repo)
        {
            _usuario = usuario;
            _repo = repo;
        }

        public async Task<CadastrarContaResult> Handle(CadastrarContaCommand request, CancellationToken cancellationToken)
        {
            var cadastro = await _usuario.Cadastrar(request.Cpf, request.Senha, request.Nome);
            if (!cadastro.Sucesso)
            {
                return new CadastrarContaResult(false, cadastro.TipoFalha, cadastro.Mensagem, string.Empty);
            }

            var existente = await _repo.ObterPorNumero(cadastro.NumeroConta);
            if (existente != null)
            {
                return new CadastrarContaResult(true, string.Empty, string.Empty, cadastro.NumeroConta);
            }

            var conta = new ContaCorrenteDomain
            {
                NumeroConta = cadastro.NumeroConta,
                Nome = string.IsNullOrWhiteSpace(request.Nome) ? "Titular" : request.Nome.Trim(),
                Ativo = true
            };
            await _repo.Criar(conta);
            return new CadastrarContaResult(true, string.Empty, string.Empty, cadastro.NumeroConta);
        }
    }
}

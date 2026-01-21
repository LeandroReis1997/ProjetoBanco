using MediatR;
using ContaCorrente.Api.Infrastructure.Repositories;

namespace ContaCorrente.Api.Application.Queries
{
    public record SaldoQuery(string NumeroConta) : IRequest<SaldoResult>;

    public record SaldoResult(bool Sucesso, string TipoFalha, string Mensagem, string NumeroConta, string Nome, DateTime DataHora, decimal Saldo);

    public class SaldoHandler : IRequestHandler<SaldoQuery, SaldoResult>
    {
        private readonly ContaCorrenteRepository _contas;
        private readonly MovimentoRepository _movs;
        public SaldoHandler(ContaCorrenteRepository contas, MovimentoRepository movs)
        {
            _contas = contas;
            _movs = movs;
        }

        public async Task<SaldoResult> Handle(SaldoQuery request, CancellationToken cancellationToken)
        {
            var conta = await _contas.ObterPorNumero(request.NumeroConta);
            if (conta == null)
            {
                return new SaldoResult(false, "INVALID_ACCOUNT", "Conta não encontrada", string.Empty, string.Empty, DateTime.UtcNow, 0m);
            }
            if (!conta.Ativo)
            {
                return new SaldoResult(false, "INACTIVE_ACCOUNT", "Conta inativa", string.Empty, string.Empty, DateTime.UtcNow, 0m);
            }
            var saldo = await _movs.ObterSaldo(conta.Id);
            return new SaldoResult(true, string.Empty, string.Empty, conta.NumeroConta, conta.Nome, DateTime.UtcNow, saldo);
        }
    }
}

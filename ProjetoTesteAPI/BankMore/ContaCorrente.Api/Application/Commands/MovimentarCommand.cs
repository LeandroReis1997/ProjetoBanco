using MediatR;
using ContaCorrente.Api.Domain;
using ContaCorrente.Api.Infrastructure.Repositories;
using ContaCorrente.Api.Infrastructure.Services;
using System.Text.Json;

namespace ContaCorrente.Api.Application.Commands
{
    public record MovimentarCommand(string IdRequisicao, string NumeroContaToken, string Token, string? NumeroConta, decimal Valor, string Tipo) : IRequest<MovimentarResult>;

    public record MovimentarResult(bool Sucesso, string TipoFalha, string Mensagem, bool Idempotente);

    public class MovimentarHandler : IRequestHandler<MovimentarCommand, MovimentarResult>
    {
        private readonly ContaCorrenteRepository _contas;
        private readonly MovimentoRepository _movs;
        private readonly IdempotenciaRepository _idem;
        public MovimentarHandler(ContaCorrenteRepository contas, MovimentoRepository movs, IdempotenciaRepository idem)
        {
            _contas = contas;
            _movs = movs;
            _idem = idem;
        }

        public async Task<MovimentarResult> Handle(MovimentarCommand request, CancellationToken cancellationToken)
        {
            var chave = request.IdRequisicao;
            if (string.IsNullOrWhiteSpace(chave))
            {
                return new MovimentarResult(false, "INVALID_VALUE", "Identificação da requisição inválida", false);
            }
            var salvo = await _idem.ObterResultado(chave);
            if (!string.IsNullOrWhiteSpace(salvo))
            {
                var obj = JsonSerializer.Deserialize<MovimentarResult>(salvo);
                if (obj != null)
                {
                    return obj with { Idempotente = true };
                }
                return new MovimentarResult(true, string.Empty, string.Empty, true);
            }

            var requisicaoJson = JsonSerializer.Serialize(request);

            if (request.Valor <= 0)
            {
                return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(false, "INVALID_VALUE", "Valor inválido", false));
            }

            var tipo = request.Tipo?.Trim().ToUpperInvariant() ?? string.Empty;
            if (tipo != "C" && tipo != "D")
            {
                return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(false, "INVALID_TYPE", "Tipo inválido", false));
            }

            var numeroAlvo = string.IsNullOrWhiteSpace(request.NumeroConta) ? request.NumeroContaToken : request.NumeroConta.Trim();
            if (numeroAlvo != request.NumeroContaToken && tipo != "C")
            {
                return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(false, "INVALID_TYPE", "Tipo inválido para conta de terceiros", false));
            }

            var conta = await _contas.ObterPorNumero(numeroAlvo);
            if (conta == null)
            {
                return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(false, "INVALID_ACCOUNT", "Conta não encontrada", false));
            }

            if (!conta.Ativo)
            {
                return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(false, "INACTIVE_ACCOUNT", "Conta inativa", false));
            }

            var mov = new Movimento
            {
                ContaId = conta.Id,
                DataMovimento = DateTime.UtcNow,
                TipoMovimento = tipo,
                Valor = request.Valor
            };

            await _movs.Inserir(mov);
            return await SalvarResultado(chave, requisicaoJson, new MovimentarResult(true, string.Empty, string.Empty, false));
        }

        private async Task<MovimentarResult> SalvarResultado(string chave, string requisicao, MovimentarResult result)
        {
            var json = JsonSerializer.Serialize(result);
            await _idem.Salvar(chave, requisicao, json);
            return result;
        }
    }
}

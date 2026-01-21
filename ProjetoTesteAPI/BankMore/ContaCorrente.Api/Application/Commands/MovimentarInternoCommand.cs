using MediatR;
using ContaCorrente.Api.Domain;
using ContaCorrente.Api.Infrastructure.Repositories;
using System.Text.Json;

namespace ContaCorrente.Api.Application.Commands
{
    public record MovimentarInternoCommand(string IdRequisicao, long ContaId, decimal Valor, string Tipo) : IRequest<MovimentarResult>;

    public class MovimentarInternoHandler : IRequestHandler<MovimentarInternoCommand, MovimentarResult>
    {
        private readonly ContaCorrenteRepository _contas;
        private readonly MovimentoRepository _movs;
        private readonly IdempotenciaRepository _idem;

        public MovimentarInternoHandler(ContaCorrenteRepository contas, MovimentoRepository movs, IdempotenciaRepository idem)
        {
            _contas = contas;
            _movs = movs;
            _idem = idem;
        }

        public async Task<MovimentarResult> Handle(MovimentarInternoCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IdRequisicao))
            {
                return new MovimentarResult(false, "INVALID_VALUE", "Identificação da requisição inválida", false);
            }

            var salvo = await _idem.ObterResultado(request.IdRequisicao);
            if (!string.IsNullOrWhiteSpace(salvo))
            {
                var obj = JsonSerializer.Deserialize<MovimentarResult>(salvo);
                if (obj != null)
                {
                    return obj with { Idempotente = true };
                }
                return new MovimentarResult(true, string.Empty, string.Empty, true);
            }

            if (request.Valor <= 0)
            {
                return await SalvarResultado(request, new MovimentarResult(false, "INVALID_VALUE", "Valor inválido", false));
            }

            var tipo = request.Tipo?.Trim().ToUpperInvariant() ?? string.Empty;
            if (tipo != "C" && tipo != "D")
            {
                return await SalvarResultado(request, new MovimentarResult(false, "INVALID_TYPE", "Tipo inválido", false));
            }

            var conta = await _contas.ObterPorId(request.ContaId);
            if (conta == null)
            {
                return await SalvarResultado(request, new MovimentarResult(false, "INVALID_ACCOUNT", "Conta não encontrada", false));
            }

            if (!conta.Ativo)
            {
                return await SalvarResultado(request, new MovimentarResult(false, "INACTIVE_ACCOUNT", "Conta inativa", false));
            }

            var mov = new Movimento
            {
                ContaId = conta.Id,
                DataMovimento = DateTime.UtcNow,
                TipoMovimento = tipo,
                Valor = request.Valor
            };

            await _movs.Inserir(mov);
            return await SalvarResultado(request, new MovimentarResult(true, string.Empty, string.Empty, false));
        }

        private async Task<MovimentarResult> SalvarResultado(MovimentarInternoCommand request, MovimentarResult result)
        {
            var requisicao = JsonSerializer.Serialize(request);
            var resultado = JsonSerializer.Serialize(result);
            await _idem.Salvar(request.IdRequisicao, requisicao, resultado);
            return result;
        }
    }
}

using System.Text.Json;
using MediatR;
using TransferenciaDomain = Transferencia.Api.Domain.Transferencia;
using Transferencia.Api.Infrastructure.Repositories;
using Transferencia.Api.Infrastructure.Services;
using Transferencia.Api.Infrastructure.Messaging;

namespace Transferencia.Api.Application.Commands
{
    public record TransferirCommand(string IdRequisicao, string NumeroContaOrigem, string NumeroContaDestino, decimal Valor, string Token) : IRequest<TransferirResult>;

    public record TransferirResult(bool Sucesso, string TipoFalha, string Mensagem, bool Idempotente);

    public class TransferirHandler : IRequestHandler<TransferirCommand, TransferirResult>
    {
        private readonly TransferenciaRepository _repo;
        private readonly IdempotenciaRepository _idem;
        private readonly ContaCorrenteApiClient _contaApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ITransferenciaProducer _producer;

        public TransferirHandler(TransferenciaRepository repo, IdempotenciaRepository idem, ContaCorrenteApiClient contaApi, UsuarioApiClient usuarioApi, ITransferenciaProducer producer)
        {
            _repo = repo;
            _idem = idem;
            _contaApi = contaApi;
            _usuarioApi = usuarioApi;
            _producer = producer;
        }

        public async Task<TransferirResult> Handle(TransferirCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IdRequisicao))
            {
                return new TransferirResult(false, "INVALID_VALUE", "Identificação da requisição inválida", false);
            }
            var salvo = await _idem.ObterResultado(request.IdRequisicao);
            if (!string.IsNullOrWhiteSpace(salvo))
            {
                var obj = JsonSerializer.Deserialize<TransferirResult>(salvo);
                if (obj != null)
                {
                    return obj with { Idempotente = true };
                }
                return new TransferirResult(true, string.Empty, string.Empty, true);
            }

            if (request.Valor <= 0)
            {
                return await SalvarResultado(request, new TransferirResult(false, "INVALID_VALUE", "Valor inválido", false));
            }

            var destino = await _usuarioApi.ObterContaPorNumero(request.Token, request.NumeroContaDestino);
            if (!destino.Sucesso)
            {
                return await SalvarResultado(request, new TransferirResult(false, destino.TipoFalha, destino.Mensagem, false));
            }

            if (!long.TryParse(request.NumeroContaOrigem, out var origemId))
            {
                return await SalvarResultado(request, new TransferirResult(false, "INVALID_ACCOUNT", "Conta origem inválida", false));
            }

            var debito = await _contaApi.MovimentarInterno(request.Token, request.IdRequisicao, origemId, request.Valor, "D");
            if (!debito.Sucesso)
            {
                return await SalvarResultado(request, new TransferirResult(false, debito.TipoFalha, debito.Mensagem, false));
            }

            var credito = await _contaApi.MovimentarInterno(request.Token, request.IdRequisicao, destino.IdConta, request.Valor, "C");
            if (!credito.Sucesso)
            {
                await _contaApi.MovimentarInterno(request.Token, request.IdRequisicao + "-estorno", origemId, request.Valor, "C");
                return await SalvarResultado(request, new TransferirResult(false, credito.TipoFalha, credito.Mensagem, false));
            }

            var transferencia = new TransferenciaDomain
            {
                ContaOrigemId = origemId,
                ContaDestinoId = destino.IdConta,
                DataMovimento = DateTime.UtcNow,
                Valor = request.Valor
            };

            await _repo.Inserir(transferencia);
            await _producer.Publicar(new TransferenciaRealizadaMessage(request.IdRequisicao, origemId, destino.IdConta, request.Valor, DateTime.UtcNow));
            return await SalvarResultado(request, new TransferirResult(true, string.Empty, string.Empty, false));
        }

        private async Task<TransferirResult> SalvarResultado(TransferirCommand request, TransferirResult result)
        {
            var requisicao = JsonSerializer.Serialize(request);
            var resultado = JsonSerializer.Serialize(result);
            await _idem.Salvar(request.IdRequisicao, requisicao, resultado);
            return result;
        }
    }
}

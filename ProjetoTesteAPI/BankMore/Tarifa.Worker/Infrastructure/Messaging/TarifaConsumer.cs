using KafkaFlow;
using Tarifa.Worker.Infrastructure.Repositories;

namespace Tarifa.Worker.Infrastructure.Messaging
{
    public class TarifaConsumer : IMessageHandler<TransferenciaRealizadaMessage>
    {
        private readonly TarifaRepository _repo;
        private readonly TarifaProducer _producer;
        private readonly IConfiguration _config;
        public TarifaConsumer(TarifaRepository repo, TarifaProducer producer, IConfiguration config)
        {
            _repo = repo;
            _producer = producer;
            _config = config;
        }

        public async Task Handle(IMessageContext context, TransferenciaRealizadaMessage message)
        {
            var valor = decimal.TryParse(_config["Tarifa:Valor"], out var v) ? v : 2m;
            var dataHora = DateTime.UtcNow;
            await _repo.Inserir(message.ContaOrigemId, valor, dataHora);
            await _producer.Publicar(new TarifaRealizadaMessage(message.IdRequisicao, message.ContaOrigemId, valor, dataHora));
        }
    }
}

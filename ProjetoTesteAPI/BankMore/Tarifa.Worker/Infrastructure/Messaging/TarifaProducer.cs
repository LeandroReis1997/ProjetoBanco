using KafkaFlow;
using KafkaFlow.Producers;

namespace Tarifa.Worker.Infrastructure.Messaging
{
    public class TarifaProducer
    {
        private readonly IProducerAccessor _accessor;
        public TarifaProducer(IProducerAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task Publicar(TarifaRealizadaMessage message)
        {
            var producer = _accessor.GetProducer("tarifas-producer");
            return producer.ProduceAsync(message.IdRequisicao, message);
        }
    }
}

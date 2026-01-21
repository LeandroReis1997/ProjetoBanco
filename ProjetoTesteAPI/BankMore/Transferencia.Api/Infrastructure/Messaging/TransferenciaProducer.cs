using KafkaFlow;
using KafkaFlow.Producers;

namespace Transferencia.Api.Infrastructure.Messaging
{
    public class TransferenciaProducer : ITransferenciaProducer
    {
        private readonly IProducerAccessor _accessor;
        public TransferenciaProducer(IProducerAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task Publicar(TransferenciaRealizadaMessage message)
        {
            var producer = _accessor.GetProducer("transferencias-producer");
            return producer.ProduceAsync(message.IdRequisicao, message);
        }
    }
}

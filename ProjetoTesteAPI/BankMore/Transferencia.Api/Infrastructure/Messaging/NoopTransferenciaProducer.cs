using System.Threading.Tasks;

namespace Transferencia.Api.Infrastructure.Messaging
{
    public class NoopTransferenciaProducer : ITransferenciaProducer
    {
        public Task Publicar(TransferenciaRealizadaMessage message)
        {
            return Task.CompletedTask;
        }
    }
}

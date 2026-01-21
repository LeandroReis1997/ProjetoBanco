using System.Threading.Tasks;

namespace Transferencia.Api.Infrastructure.Messaging
{
    public interface ITransferenciaProducer
    {
        Task Publicar(TransferenciaRealizadaMessage message);
    }
}

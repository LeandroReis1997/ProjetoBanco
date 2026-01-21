using KafkaFlow;
using MediatR;
using ContaCorrente.Api.Application.Commands;

namespace ContaCorrente.Api.Infrastructure.Messaging
{
    public class TarifaConsumer : IMessageHandler<TarifaRealizadaMessage>
    {
        private readonly IMediator _mediator;
        public TarifaConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(IMessageContext context, TarifaRealizadaMessage message)
        {
            var cmd = new MovimentarInternoCommand(message.IdRequisicao + "-tarifa", message.ContaId, message.Valor, "D");
            await _mediator.Send(cmd);
        }
    }
}

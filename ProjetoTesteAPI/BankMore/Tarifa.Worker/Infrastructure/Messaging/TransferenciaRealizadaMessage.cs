namespace Tarifa.Worker.Infrastructure.Messaging
{
    public record TransferenciaRealizadaMessage(string IdRequisicao, long ContaOrigemId, long ContaDestinoId, decimal Valor, DateTime DataHora);
}

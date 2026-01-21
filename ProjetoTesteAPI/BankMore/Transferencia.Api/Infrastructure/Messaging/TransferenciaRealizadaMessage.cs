namespace Transferencia.Api.Infrastructure.Messaging
{
    public record TransferenciaRealizadaMessage(string IdRequisicao, long ContaOrigemId, long ContaDestinoId, decimal Valor, DateTime DataHora);
}

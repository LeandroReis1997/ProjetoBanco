namespace Tarifa.Worker.Infrastructure.Messaging
{
    public record TarifaRealizadaMessage(string IdRequisicao, long ContaId, decimal Valor, DateTime DataHora);
}

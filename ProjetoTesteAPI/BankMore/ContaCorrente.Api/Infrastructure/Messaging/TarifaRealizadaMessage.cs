namespace ContaCorrente.Api.Infrastructure.Messaging
{
    public record TarifaRealizadaMessage(string IdRequisicao, long ContaId, decimal Valor, DateTime DataHora);
}

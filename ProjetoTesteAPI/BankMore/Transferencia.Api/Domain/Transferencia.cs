namespace Transferencia.Api.Domain
{
    public class Transferencia
    {
        public long Id { get; set; }
        public long ContaOrigemId { get; set; }
        public long ContaDestinoId { get; set; }
        public DateTime DataMovimento { get; set; }
        public decimal Valor { get; set; }
    }
}

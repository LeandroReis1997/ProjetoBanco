namespace ContaCorrente.Api.Domain
{
    public class Movimento
    {
        public long Id { get; set; }
        public long ContaId { get; set; }
        public DateTime DataMovimento { get; set; }
        public string TipoMovimento { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}

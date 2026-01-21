namespace ContaCorrente.Api.Domain
{
    public class ContaCorrente
    {
        public long Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}

namespace Usuario.Api.Domain
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}

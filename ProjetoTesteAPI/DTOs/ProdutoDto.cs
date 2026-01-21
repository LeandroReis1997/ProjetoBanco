using System.ComponentModel.DataAnnotations;

namespace ProjetoTesteAPI.DTOs
{
    public class ProdutoDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Preco { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantidade { get; set; }
    }
}

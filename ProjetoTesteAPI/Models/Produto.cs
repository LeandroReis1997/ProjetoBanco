using System.ComponentModel.DataAnnotations;

namespace ProjetoTesteAPI.Models
{
    public class Produto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;
        [Required]
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}

using Microsoft.Extensions.DependencyInjection;
using ProjetoTesteAPI.Models;

namespace ProjetoTesteAPI.Data
{
    public static class SeedData
    {
        public static void Inicializar(IServiceProvider services)
        {
            using var escopo = services.CreateScope();
            var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Produtos.Any())
            {
                db.Produtos.AddRange(
                    new Produto { Nome = "Caneta", Preco = 2.50M, Quantidade = 150 },
                    new Produto { Nome = "Caderno", Preco = 12.90M, Quantidade = 80 },
                    new Produto { Nome = "Lapis", Preco = 1.20M, Quantidade = 300 }
                );
                db.SaveChanges();
            }
        }
    }
}

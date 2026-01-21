using Microsoft.EntityFrameworkCore;
using ProjetoTesteAPI.Models;

namespace ProjetoTesteAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Produto> Produtos { get; set; }
    }
}

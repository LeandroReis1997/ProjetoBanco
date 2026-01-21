using Microsoft.EntityFrameworkCore;
using ProjetoTesteAPI.Data;
using ProjetoTesteAPI.Models;

namespace ProjetoTesteAPI.Repositories
{
    public class RepositorioProduto : IRepositorioProduto
    {
        private readonly AppDbContext _context;
        public RepositorioProduto(AppDbContext context) { _context = context; }
        public async Task<IEnumerable<Produto>> ObterTodos()
        {
            return await _context.Produtos.AsNoTracking().ToListAsync();
        }
        public async Task<Produto?> ObterPorId(int id)
        {
            return await _context.Produtos.FindAsync(id);
        }
        public async Task<Produto> Criar(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return produto;
        }
        public async Task Atualizar(Produto produto)
        {
            _context.Entry(produto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task Deletar(int id)
        {
            var existente = await _context.Produtos.FindAsync(id);
            if (existente != null)
            {
                _context.Produtos.Remove(existente);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using ProjetoTesteAPI.Models;

namespace ProjetoTesteAPI.Repositories
{
    public interface IRepositorioProduto
    {
        Task<IEnumerable<Produto>> ObterTodos();
        Task<Produto?> ObterPorId(int id);
        Task<Produto> Criar(Produto produto);
        Task Atualizar(Produto produto);
        Task Deletar(int id);
    }
}

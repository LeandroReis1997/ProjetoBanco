using ProjetoTesteAPI.DTOs;
using ProjetoTesteAPI.Models;
using ProjetoTesteAPI.Repositories;

namespace ProjetoTesteAPI.Services
{
    public class ServicoProduto
    {
        private readonly IRepositorioProduto _repositorio;
        public ServicoProduto(IRepositorioProduto repositorio) { _repositorio = repositorio; }

        public async Task<IEnumerable<ProdutoDto>> ObterTodos()
        {
            var list = await _repositorio.ObterTodos();
            return list.Select(p => new ProdutoDto { Id = p.Id, Nome = p.Nome, Preco = p.Preco, Quantidade = p.Quantidade });
        }

        public async Task<ProdutoDto?> ObterPorId(int id)
        {
            var p = await _repositorio.ObterPorId(id);
            if (p == null) return null;
            return new ProdutoDto { Id = p.Id, Nome = p.Nome, Preco = p.Preco, Quantidade = p.Quantidade };
        }

        public async Task<ProdutoDto> Criar(ProdutoDto dto)
        {
            var p = new Produto { Nome = dto.Nome, Preco = dto.Preco, Quantidade = dto.Quantidade };
            var criado = await _repositorio.Criar(p);
            return new ProdutoDto { Id = criado.Id, Nome = criado.Nome, Preco = criado.Preco, Quantidade = criado.Quantidade };
        }

        public async Task<bool> Atualizar(int id, ProdutoDto dto)
        {
            var existente = await _repositorio.ObterPorId(id);
            if (existente == null) return false;
            existente.Nome = dto.Nome;
            existente.Preco = dto.Preco;
            existente.Quantidade = dto.Quantidade;
            await _repositorio.Atualizar(existente);
            return true;
        }

        public async Task<bool> Deletar(int id)
        {
            var existente = await _repositorio.ObterPorId(id);
            if (existente == null) return false;
            await _repositorio.Deletar(id);
            return true;
        }
    }
}

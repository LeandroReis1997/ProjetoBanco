using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Usuario.Api.Infrastructure.Repositories;

namespace Usuario.Api.Application.Queries
{
    public record ObterContaPorNumeroQuery(string NumeroConta) : IRequest<ObterContaPorNumeroResult>;

    public record ObterContaPorNumeroResult(bool Sucesso, string TipoFalha, string Mensagem, long IdConta, string Nome);

    public class ObterContaPorNumeroHandler : IRequestHandler<ObterContaPorNumeroQuery, ObterContaPorNumeroResult>
    {
        private readonly UsuarioRepository _repo;
        private readonly IMemoryCache _cache;
        public ObterContaPorNumeroHandler(UsuarioRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<ObterContaPorNumeroResult> Handle(ObterContaPorNumeroQuery request, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue("conta:" + request.NumeroConta, out ObterContaPorNumeroResult? cached) && cached != null)
            {
                return cached;
            }

            var usuario = await _repo.ObterPorNumero(request.NumeroConta);
            if (usuario == null)
            {
                return new ObterContaPorNumeroResult(false, "INVALID_ACCOUNT", "Conta não encontrada", 0, string.Empty);
            }

            var result = new ObterContaPorNumeroResult(true, string.Empty, string.Empty, usuario.Id, usuario.Nome);
            _cache.Set("conta:" + request.NumeroConta, result, TimeSpan.FromMinutes(2));
            return result;
        }
    }
}

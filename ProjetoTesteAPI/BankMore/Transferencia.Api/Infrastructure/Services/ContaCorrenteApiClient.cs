using System.Net.Http.Json;

namespace Transferencia.Api.Infrastructure.Services
{
    public class ContaCorrenteApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        public ContaCorrenteApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string BaseUrl => _config["ContaCorrenteApi:BaseUrl"] ?? "http://localhost:6002";

        public async Task<MovimentarResult> Movimentar(string token, string idRequisicao, string? numeroConta, decimal valor, string tipo)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/contas/movimentar");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            req.Content = JsonContent.Create(new { idRequisicao, numeroConta, valor, tipo });
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                return new MovimentarResult(true, string.Empty, string.Empty);
            }
            var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("INVALID_VALUE", "Falha ao movimentar");
            return new MovimentarResult(false, erro.Tipo, erro.Mensagem);
        }

        public async Task<MovimentarResult> MovimentarInterno(string token, string idRequisicao, long contaId, decimal valor, string tipo)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/contas/movimentar-interno");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            req.Content = JsonContent.Create(new { idRequisicao, contaId, valor, tipo });
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                return new MovimentarResult(true, string.Empty, string.Empty);
            }
            var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("INVALID_VALUE", "Falha ao movimentar");
            return new MovimentarResult(false, erro.Tipo, erro.Mensagem);
        }

        public record ErroResponse(string Tipo, string Mensagem);
        public record MovimentarResult(bool Sucesso, string TipoFalha, string Mensagem);
    }
}

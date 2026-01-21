using System.Net.Http.Json;

namespace Transferencia.Api.Infrastructure.Services
{
    public class UsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        public UsuarioApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string BaseUrl => _config["UsuarioApi:BaseUrl"] ?? "http://localhost:6001";

        public async Task<ContaResult> ObterContaPorNumero(string token, string numeroConta)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/usuarios/conta/{numeroConta}");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("INVALID_ACCOUNT", "Conta inválida");
                return new ContaResult(false, erro.Tipo, erro.Mensagem, 0, string.Empty);
            }

            var ok = await resp.Content.ReadFromJsonAsync<ContaResponse>() ?? new ContaResponse(0, "");
            return new ContaResult(true, string.Empty, string.Empty, ok.IdConta, ok.Nome);
        }

        public record ErroResponse(string Tipo, string Mensagem);
        public record ContaResponse(long IdConta, string Nome);
        public record ContaResult(bool Sucesso, string TipoFalha, string Mensagem, long IdConta, string Nome);
    }
}

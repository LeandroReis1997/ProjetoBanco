using System.Net.Http.Json;

namespace ContaCorrente.Api.Infrastructure.Services
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

        public async Task<CadastroResult> Cadastrar(string cpf, string senha, string? nome)
        {
            var resp = await _http.PostAsJsonAsync($"{BaseUrl}/api/usuarios/cadastrar", new { cpf, senha, nome });
            if (!resp.IsSuccessStatusCode)
            {
                var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("INVALID_DOCUMENT", "Erro ao cadastrar");
                return new CadastroResult(false, erro.Tipo, erro.Mensagem, string.Empty);
            }

            var ok = await resp.Content.ReadFromJsonAsync<CadastroResponse>() ?? new CadastroResponse("");
            return new CadastroResult(true, string.Empty, string.Empty, ok.NumeroConta);
        }

        public async Task<LoginResult> Login(string cpfOuNumero, string senha)
        {
            var resp = await _http.PostAsJsonAsync($"{BaseUrl}/api/usuarios/login", new { cpfOuNumero, senha });
            if (!resp.IsSuccessStatusCode)
            {
                var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("USER_UNAUTHORIZED", "Usuário não autorizado");
                return new LoginResult(false, erro.Tipo, erro.Mensagem, string.Empty, string.Empty, string.Empty);
            }

            var ok = await resp.Content.ReadFromJsonAsync<LoginResponse>() ?? new LoginResponse("", "", "");
            return new LoginResult(true, string.Empty, string.Empty, ok.Token, ok.NumeroConta, ok.Nome);
        }

        public async Task<ValidarSenhaResult> ValidarSenha(string token, string senha)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/usuarios/validar-senha");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            req.Content = JsonContent.Create(new { senha });
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                return new ValidarSenhaResult(true, string.Empty, string.Empty);
            }
            var erro = await resp.Content.ReadFromJsonAsync<ErroResponse>() ?? new ErroResponse("USER_UNAUTHORIZED", "Senha inválida");
            return new ValidarSenhaResult(false, erro.Tipo, erro.Mensagem);
        }

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
        public record CadastroResponse(string NumeroConta);
        public record LoginResponse(string Token, string NumeroConta, string Nome);
        public record ContaResponse(long IdConta, string Nome);
        public record CadastroResult(bool Sucesso, string TipoFalha, string Mensagem, string NumeroConta);
        public record LoginResult(bool Sucesso, string TipoFalha, string Mensagem, string Token, string NumeroConta, string Nome);
        public record ValidarSenhaResult(bool Sucesso, string TipoFalha, string Mensagem);
        public record ContaResult(bool Sucesso, string TipoFalha, string Mensagem, long IdConta, string Nome);
    }
}

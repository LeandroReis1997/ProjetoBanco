using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ProjetoTesteAPI.Middlewares
{
    public class TratamentoExcecoesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TratamentoExcecoesMiddleware> _logger;
        public TratamentoExcecoesMiddleware(RequestDelegate next, ILogger<TratamentoExcecoesMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var resultado = JsonSerializer.Serialize(new { mensagem = "Ocorreu um erro interno." });
                await context.Response.WriteAsync(resultado);
            }
        }
    }

    public static class AplicacaoExtensoes
    {
        public static IApplicationBuilder UseTratamentoExcecoes(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TratamentoExcecoesMiddleware>();
        }
    }
}

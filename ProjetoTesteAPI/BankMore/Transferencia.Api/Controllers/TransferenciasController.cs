using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transferencia.Api.Application.Commands;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Transferencia.Api.Controllers
{
    [ApiController]
    [Route("api/transferencias")]
    public class TransferenciasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TransferenciasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Efetuar transferência", Description = "Transferência entre contas da mesma instituição")]
        [SwaggerRequestExample(typeof(TransferirRequest), typeof(TransferirRequestExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Transferência realizada")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Transferir([FromBody] TransferirRequest request)
        {
            var numeroContaOrigem = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? User.FindFirstValue("sub")
                                   ?? string.Empty;
            var token = ObterToken();
            if (string.IsNullOrWhiteSpace(numeroContaOrigem) || string.IsNullOrWhiteSpace(token))
            {
                return Forbid();
            }

            var cmd = new TransferirCommand(request.IdRequisicao, numeroContaOrigem, request.NumeroContaDestino, request.Valor, token);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }
            return NoContent();
        }

        private string ObterToken()
        {
            var auth = Request.Headers.Authorization.ToString();
            return auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : string.Empty;
        }
    }

    public record TransferirRequest(string IdRequisicao, string NumeroContaDestino, decimal Valor);
    public record ErroResponse(string Tipo, string Mensagem);
}

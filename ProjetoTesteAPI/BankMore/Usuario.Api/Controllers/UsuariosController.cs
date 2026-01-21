using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usuario.Api.Application.Commands;
using Usuario.Api.Application.Queries;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Usuario.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("cadastrar")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Cadastrar conta", Description = "Cria uma conta e retorna o número da conta corrente")]
        [SwaggerRequestExample(typeof(CadastrarUsuarioRequest), typeof(CadastrarUsuarioRequestExample))]
        [SwaggerResponse(StatusCodes.Status200OK, "Conta criada", typeof(object))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(NumeroContaResponseExample))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "CPF inválido", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarUsuarioRequest request)
        {
            var cmd = new CadastrarUsuarioCommand(request.Cpf, request.Senha, request.Nome ?? string.Empty);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }

            return Ok(new { numeroConta = result.NumeroConta });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Efetuar login", Description = "Retorna JWT com identificação da conta")]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [SwaggerResponse(StatusCodes.Status200OK, "Login efetuado", typeof(object))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseExample))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Usuário não autorizado", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(ErroResponseExample))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var cmd = new LoginCommand(request.CpfOuNumero, request.Senha);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return Unauthorized(new ErroResponse(result.TipoFalha, result.Mensagem));
            }

            return Ok(new { token = result.Token, numeroConta = result.NumeroConta, nome = result.Nome });
        }

        [HttpPost("validar-senha")]
        [Authorize]
        [SwaggerOperation(Summary = "Validar senha", Description = "Valida senha da conta autenticada")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Senha válida")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Senha inválida", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(ErroResponseExample))]
        public async Task<IActionResult> ValidarSenha([FromBody] ValidarSenhaRequest request)
        {
            var id = ObterIdConta();
            if (id == 0)
            {
                return Forbid();
            }

            var cmd = new ValidarSenhaCommand(id, request.Senha);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return Unauthorized(new ErroResponse(result.TipoFalha, result.Mensagem));
            }

            return NoContent();
        }

        [HttpGet("conta/{numeroConta}")]
        [Authorize]
        [SwaggerOperation(Summary = "Obter conta por número", Description = "Retorna identificação interna e nome da conta")]
        [SwaggerResponse(StatusCodes.Status200OK, "Conta encontrada", typeof(object))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Conta inválida", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> ObterContaPorNumero([FromRoute] string numeroConta)
        {
            var query = new ObterContaPorNumeroQuery(numeroConta);
            var result = await _mediator.Send(query);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }

            return Ok(new { idConta = result.IdConta, nome = result.Nome });
        }

        private long ObterIdConta()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")
                      ?? "0";

            return long.TryParse(sub, out var id) ? id : 0;
        }
    }

    public record CadastrarUsuarioRequest(string Cpf, string Senha, string? Nome);
    public record LoginRequest(string CpfOuNumero, string Senha);
    public record ValidarSenhaRequest(string Senha);
    public record ErroResponse(string Tipo, string Mensagem);
}

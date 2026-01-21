using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContaCorrente.Api.Application.Commands;
using ContaCorrente.Api.Application.Queries;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ContaCorrente.Api.Controllers
{
    [ApiController]
    [Route("api/contas")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContaCorrenteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("cadastrar")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Cadastrar conta", Description = "Cria conta e retorna número")]
        [SwaggerRequestExample(typeof(CadastrarContaRequest), typeof(CadastrarContaRequestExample))]
        [SwaggerResponse(StatusCodes.Status200OK, "Conta criada", typeof(object))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(NumeroContaResponseExample))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "CPF inválido", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarContaRequest request)
        {
            var cmd = new CadastrarContaCommand(request.Cpf, request.Senha, request.Nome ?? string.Empty);
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

        [HttpPost("inativar")]
        [Authorize]
        [SwaggerOperation(Summary = "Inativar conta", Description = "Inativa conta autenticada")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Conta inativada")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Conta inválida", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Inativar([FromBody] InativarContaRequest request)
        {
            var numeroConta = ObterNumeroConta();
            var token = ObterToken();
            if (string.IsNullOrWhiteSpace(numeroConta) || string.IsNullOrWhiteSpace(token))
            {
                return Forbid();
            }
            var cmd = new InativarContaCommand(token, numeroConta, request.Senha);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                if (result.TipoFalha == "INVALID_ACCOUNT" || result.TipoFalha == "INACTIVE_ACCOUNT")
                    return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
                return Unauthorized(new ErroResponse(result.TipoFalha, result.Mensagem));
            }
            return NoContent();
        }

        [HttpPost("movimentar")]
        [Authorize]
        [SwaggerOperation(Summary = "Movimentar conta", Description = "Crédito ou débito na conta")]
        [SwaggerRequestExample(typeof(MovimentarRequest), typeof(MovimentarRequestExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Movimentação realizada")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Movimentar([FromBody] MovimentarRequest request)
        {
            var numeroConta = ObterNumeroConta();
            var token = ObterToken();
            if (string.IsNullOrWhiteSpace(numeroConta) || string.IsNullOrWhiteSpace(token))
            {
                return Forbid();
            }
            var cmd = new MovimentarCommand(request.IdRequisicao, numeroConta, token, request.NumeroConta, request.Valor, request.Tipo);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }
            return NoContent();
        }

        [HttpPost("movimentar-interno")]
        [Authorize]
        [SwaggerOperation(Summary = "Movimentar interno", Description = "Uso interno com contaId")]
        [SwaggerRequestExample(typeof(MovimentarInternoRequest), typeof(MovimentarInternoRequestExample))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Movimentação realizada")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> MovimentarInterno([FromBody] MovimentarInternoRequest request)
        {
            var token = ObterToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return Forbid();
            }
            var cmd = new MovimentarInternoCommand(request.IdRequisicao, request.ContaId, request.Valor, request.Tipo);
            var result = await _mediator.Send(cmd);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }
            return NoContent();
        }

        [HttpGet("saldo")]
        [Authorize]
        [SwaggerOperation(Summary = "Consultar saldo", Description = "Retorna saldo da conta")]
        [SwaggerResponse(StatusCodes.Status200OK, "Saldo atual", typeof(object))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SaldoResponseExample))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Conta inválida", typeof(ErroResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErroResponseExample))]
        public async Task<IActionResult> Saldo()
        {
            var numeroConta = ObterNumeroConta();
            if (string.IsNullOrWhiteSpace(numeroConta))
            {
                return Forbid();
            }
            var query = new SaldoQuery(numeroConta);
            var result = await _mediator.Send(query);
            if (!result.Sucesso)
            {
                return BadRequest(new ErroResponse(result.TipoFalha, result.Mensagem));
            }
            return Ok(new {
                numeroConta = result.NumeroConta,
                nome = result.Nome,
                dataHora = result.DataHora,
                saldo = result.Saldo
            });
        }

        private string ObterNumeroConta()
        {
            return User.FindFirstValue("numero_conta") ?? string.Empty;
        }

        private string ObterToken()
        {
            var auth = Request.Headers.Authorization.ToString();
            return auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : string.Empty;
        }
    }

    public record CadastrarContaRequest(string Cpf, string Senha, string? Nome);
    public record LoginRequest(string CpfOuNumero, string Senha);
    public record InativarContaRequest(string Senha);
    public record MovimentarRequest(string IdRequisicao, string? NumeroConta, decimal Valor, string Tipo);
    public record MovimentarInternoRequest(string IdRequisicao, long ContaId, decimal Valor, string Tipo);
    public record ErroResponse(string Tipo, string Mensagem);
}

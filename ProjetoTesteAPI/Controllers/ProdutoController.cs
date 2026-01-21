using Microsoft.AspNetCore.Mvc;
using ProjetoTesteAPI.DTOs;
using ProjetoTesteAPI.Services;

namespace ProjetoTesteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ServicoDeProduto _servico;
        public ProdutoController(ServicoDeProduto servico) { _servico = servico; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterTodos()
        {
            var result = await _servico.ObterTodos();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProdutoDto>> ObterPorId(int id)
        {
            var result = await _servico.ObterPorId(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> Criar([FromBody] ProdutoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var criado = await _servico.Criar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _servico.Atualizar(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var ok = await _servico.Deletar(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}

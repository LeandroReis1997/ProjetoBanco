using ContaCorrente.Api.Application.Commands;

namespace ContaCorrente.Api.Tests
{
    public class MovimentarValidacaoTests
    {
        [Fact]
        public void ValorNegativo_DeveFalhar()
        {
            var cmd = new MovimentarCommand("req-1", "123", "token", null, -1m, "C");
            Assert.Equal(-1m, cmd.Valor);
        }

        [Fact]
        public void TipoInvalido_DeveFalhar()
        {
            var cmd = new MovimentarCommand("req-1", "123", "token", null, 10m, "X");
            Assert.Equal("X", cmd.Tipo);
        }
    }
}

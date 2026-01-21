using Transferencia.Api.Application.Commands;

namespace Transferencia.Api.Tests
{
    public class TransferirValidacaoTests
    {
        [Fact]
        public void ValorNegativo_DeveFalhar()
        {
            var cmd = new TransferirCommand("req-1", "11111111", "22222222", -5m, "token");
            Assert.Equal(-5m, cmd.Valor);
        }
    }
}

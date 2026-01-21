using Usuario.Api.Infrastructure.Security;

namespace Usuario.Api.Tests
{
    public class CpfValidatorTests
    {
        [Fact]
        public void CpfInvalido_DeveRetornarFalse()
        {
            var ok = CpfValidator.Validar("111.111.111-11");
            Assert.False(ok);
        }

        [Fact]
        public void CpfValido_DeveRetornarTrue()
        {
            var ok = CpfValidator.Validar("529.982.247-25");
            Assert.True(ok);
        }
    }
}

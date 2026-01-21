using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Usuario.Api.Infrastructure.Security
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GerarToken(long idConta, string numeroConta, string nome)
        {
            var key = _config["Jwt:Key"] ?? "chave-dev";
            var issuer = _config["Jwt:Issuer"] ?? "BankMore";
            var audience = _config["Jwt:Audience"] ?? "BankMore";
            var minutos = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, idConta.ToString()),
                new Claim("numero_conta", numeroConta),
                new Claim("nome", nome)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(minutos), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

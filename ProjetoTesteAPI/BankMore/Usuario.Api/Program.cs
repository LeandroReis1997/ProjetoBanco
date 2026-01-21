using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi;
using Usuario.Api.Infrastructure.Db;
using Usuario.Api.Infrastructure.Repositories;
using Usuario.Api.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.ExampleFilters();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT como: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? "chave-dev";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BankMore",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BankMore",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(new { tipo = "USER_UNAUTHORIZED", mensagem = "Token inválido ou expirado" });
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Normalize Authorization header: allow pasting token without the "Bearer " prefix
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var auth = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(auth) && !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Headers["Authorization"] = "Bearer " + auth;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    var dbInit = app.Services.GetRequiredService<DbInitializer>();
    dbInit.Inicializar();
}
catch (Exception ex)
{
    Console.WriteLine("DB initialization error: " + ex);
}

app.Run();

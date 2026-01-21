using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using KafkaFlow;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi;
using Transferencia.Api.Infrastructure.Db;
using Transferencia.Api.Infrastructure.Messaging;
using Transferencia.Api.Infrastructure.Repositories;
using Transferencia.Api.Infrastructure.Services;

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
        Console.WriteLine($"JWT KEY LEN: {key.Length}, PREFIX: {key.Substring(0, Math.Min(6, key.Length))}");
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
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT AUTH FAILED: " + context.Exception);
                return Task.CompletedTask;
            },
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
builder.Services.AddScoped<TransferenciaRepository>();
builder.Services.AddScoped<IdempotenciaRepository>();
builder.Services.AddHttpClient<ContaCorrenteApiClient>();
builder.Services.AddHttpClient<UsuarioApiClient>();
builder.Services.AddMemoryCache();
// register concrete or noop implementation for ITransferenciaProducer depending on Kafka availability
if (builder.Configuration.GetValue<bool>("Kafka:Enabled", false))
{
    builder.Services.AddScoped<ITransferenciaProducer, TransferenciaProducer>();
}
else
{
    builder.Services.AddScoped<ITransferenciaProducer, NoopTransferenciaProducer>();
}

if (builder.Configuration.GetValue<bool>("Kafka:Enabled", false))
{
    builder.Services.AddKafka(kafka => kafka
        .AddCluster(cluster => cluster
            .WithBrokers(new[] { builder.Configuration["Kafka:Brokers"] ?? "localhost:9092" })
            .AddProducer("transferencias-producer", producer => producer
                .DefaultTopic(builder.Configuration["Kafka:TransferenciasTopic"] ?? "transferencias-realizadas")
                .AddMiddlewares(m => m.AddSerializer<JsonMessageSerializer>()))));
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var dbInit = app.Services.GetRequiredService<DbInitializer>();
try
{
    dbInit.Inicializar();
}
catch (Exception ex)
{
    Console.WriteLine("DB INITIALIZER ERROR: " + ex);
    throw;
}

if (builder.Configuration.GetValue<bool>("Kafka:Enabled", false))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(() => app.Services.GetRequiredService<IKafkaBus>().StartAsync());
    });
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("UNHANDLED HOST ERROR: " + ex);
    throw;
}

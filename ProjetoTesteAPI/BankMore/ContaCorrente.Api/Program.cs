using System.Text;
using KafkaFlow;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi;
using ContaCorrente.Api.Infrastructure.Db;
using ContaCorrente.Api.Infrastructure.Messaging;
using ContaCorrente.Api.Infrastructure.Repositories;
using ContaCorrente.Api.Infrastructure.Services;

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
builder.Services.AddScoped<ContaCorrenteRepository>();
builder.Services.AddScoped<MovimentoRepository>();
builder.Services.AddScoped<IdempotenciaRepository>();
builder.Services.AddHttpClient<UsuarioApiClient>();
builder.Services.AddMemoryCache();
builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { builder.Configuration["Kafka:Brokers"] ?? "localhost:9092" })
        .AddConsumer(consumer => consumer
            .Topic(builder.Configuration["Kafka:TarifasTopic"] ?? "tarifas-realizadas")
            .WithGroupId("contacorrente-tarifas")
            .WithBufferSize(1024)
            .AddMiddlewares(m => m.AddDeserializer<JsonMessageSerializer>()
                .AddTypedHandlers(h => h.AddHandler<TarifaConsumer>())))));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var dbInit = app.Services.GetRequiredService<DbInitializer>();
dbInit.Inicializar();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(() => app.Services.GetRequiredService<IKafkaBus>().StartAsync());
});

app.Run();

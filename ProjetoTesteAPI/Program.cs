using Microsoft.EntityFrameworkCore;
using ProjetoTesteAPI.Data;
using ProjetoTesteAPI.Middlewares;
using ProjetoTesteAPI.Repositories;
using ProjetoTesteAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));
builder.Services.AddScoped<IRepositorioProduto, RepositorioProduto>();
builder.Services.AddScoped<ServicoDeProduto>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

SeedData.Inicializar(app.Services);

app.UseTratamentoExcecoes();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Habilita Swagger para testes (disponível mesmo em Production durante desenvolvimento local)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

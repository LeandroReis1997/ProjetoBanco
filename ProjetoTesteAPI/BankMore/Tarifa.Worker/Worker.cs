using KafkaFlow;
using Tarifa.Worker.Infrastructure.Db;

namespace Tarifa.Worker;

public class Worker : BackgroundService
{
    private readonly IKafkaBus _bus;
    private readonly DbInitializer _db;

    public Worker(IKafkaBus bus, DbInitializer db)
    {
        _bus = bus;
        _db = db;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _db.Inicializar();
        await _bus.StartAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

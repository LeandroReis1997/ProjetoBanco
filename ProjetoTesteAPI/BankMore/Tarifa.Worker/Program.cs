using KafkaFlow;
using Tarifa.Worker;
using Tarifa.Worker.Infrastructure.Db;
using Tarifa.Worker.Infrastructure.Messaging;
using Tarifa.Worker.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddScoped<TarifaRepository>();
builder.Services.AddScoped<TarifaProducer>();

builder.Services.AddKafka(kafka => kafka
	.AddCluster(cluster => cluster
		.WithBrokers(new[] { builder.Configuration["Kafka:Brokers"] ?? "localhost:9092" })
		.AddProducer("tarifas-producer", producer => producer
			.DefaultTopic(builder.Configuration["Kafka:TarifasTopic"] ?? "tarifas-realizadas")
			.AddMiddlewares(m => m.AddSerializer<JsonMessageSerializer>()))
		.AddConsumer(consumer => consumer
			.Topic(builder.Configuration["Kafka:TransferenciasTopic"] ?? "transferencias-realizadas")
			.WithGroupId("tarifa-worker")
			.WithBufferSize(1024)
			.AddMiddlewares(m => m.AddDeserializer<JsonMessageSerializer>()
				.AddTypedHandlers(h => h.AddHandler<TarifaConsumer>())))));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

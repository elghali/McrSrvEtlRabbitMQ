using EventBus.Messages.Common;
using Loader.API;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        //services.AddHostedService<Worker>();
        services.AddHostedService<CsvLoader>();

        services.AddAutoMapper(typeof(Program));

        //MassTransit-RabbitMQ Configuration
        services.AddMassTransit(config =>
        {
            config.AddConsumer<CsvLoader>();
            config.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(context.Configuration.GetValue<string>("EventBusSettings:HostAddress"));

                cfg.ReceiveEndpoint(EventBusConstants.ParserFileQeueue, c =>
                {
                    c.ConfigureConsumer<CsvLoader>(ctx);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

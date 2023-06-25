using ExcelCreate.Worker;
using ExcelCreate.Worker.Models;
using ExcelCreate.Worker.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<AdventureWorksContext>(options =>
        {
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer"));
        });

        services.AddSingleton(sp => new ConnectionFactory()
        {
            Uri = new Uri(hostContext.Configuration.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = true
        });

        services.AddSingleton<RabbitMQClientService>();
        services.AddHostedService<Worker>();
    }).Build();

host.Run();

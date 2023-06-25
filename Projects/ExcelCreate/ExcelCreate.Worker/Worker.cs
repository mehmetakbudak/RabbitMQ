using ClosedXML.Excel;
using ExcelCreate.Worker.Models;
using ExcelCreate.Worker.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ExcelCreate.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(
            ILogger<Worker> logger,
            RabbitMQClientService rabbitMQClientService,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var strBody = Encoding.UTF8.GetString(@event.Body.ToArray());

            var excel = JsonSerializer.Deserialize<CreateExcelMessage>(strBody);

            using var ms = new MemoryStream();

            var wb = new XLWorkbook();

            var ds = new DataSet();
            var dataTable = await GetTable("Products");
            ds.Tables.Add(dataTable);
            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormData = new();
            multipartFormData.Add(new ByteArrayContent(ms.ToArray()), "file", $"{Guid.NewGuid()}.xlsx");

            var baseUrl = "http://localhost:17611/api/file";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={excel.FileId}", multipartFormData);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File ( Id: {excel.FileId} was created by successfull.)");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
        }

        private async Task<DataTable> GetTable(string tableName)
        {
            List<Models.Product> products;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorksContext>();

                products = await context.Products.ToListAsync();
            }

            DataTable table = new DataTable { TableName = tableName };

            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId, x.Name, x.ProductNumber, x.Color);
            });

            return table;
        }
    }
}
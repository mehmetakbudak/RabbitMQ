using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct);

Enum.GetNames(typeof(LogTypes)).ToList().ForEach(x =>
{
    var queueName = $"direct-queue-{x}";

    var routeKey = $"route-{x}";

    channel.QueueDeclare(queueName, true, false, false);

    channel.QueueBind(queueName, "logs-direct", routeKey, null);
});

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogTypes log = (LogTypes)new Random().Next(1, 5);

    var message = $"Message - {x}";
    
    var routeKey = $"route-{log}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish("logs-direct", routeKey, null, messageBody);

    Console.WriteLine($"Log gönderildi: {message}");
});

Console.ReadLine();

enum LogTypes
{
    Critical = 1,
    Error,
    Warning,
    Info
}
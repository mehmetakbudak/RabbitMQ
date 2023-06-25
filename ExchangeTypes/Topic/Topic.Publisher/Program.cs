using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);

Random rand = new Random();

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogTypes log1 = (LogTypes)new Random().Next(1, 5);
    LogTypes log2 = (LogTypes)new Random().Next(1, 5);
    LogTypes log3 = (LogTypes)new Random().Next(1, 5);

    var routeKey = $"{log1}.{log2}.{log3}";

    var message = $"Log type: {log1}-{log2}-{log3}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish("logs-topic", routeKey, null, messageBody);

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
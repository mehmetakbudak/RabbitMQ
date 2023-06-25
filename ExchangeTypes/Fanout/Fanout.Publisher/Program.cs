using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    var message = $"Log {x}";

    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish("logs-fanout", "", null, messageBody);

    Console.WriteLine($"Mesaj gönderildi: {message}");
});

Console.ReadLine();
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

var consumer = new EventingBasicConsumer(channel);

channel.BasicConsume("product", true, consumer);

consumer.Received += (sender, args) =>
{
    var message = Encoding.UTF8.GetString(args.Body.ToArray());
    var product = JsonSerializer.Deserialize<Product>(message);
    Console.WriteLine($"Gelen mesaj : Id: {product.Id} , Adı: {product.Name}, Fiyatı: {product.Price},  Stok: {product.Stock}");
};

Console.ReadLine();
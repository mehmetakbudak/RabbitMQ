using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.QueueDeclare("product", true, false, false);

var product = new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 123 };

var productJsonString = JsonSerializer.Serialize(product);

var messageBody = Encoding.UTF8.GetBytes(productJsonString);

channel.BasicPublish(string.Empty, "product", null, messageBody);

Console.WriteLine("Mesaj gönderildi");

Console.ReadLine();
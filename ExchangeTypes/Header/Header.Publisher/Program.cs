using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

Dictionary<string, object> headers = new Dictionary<string, object>();
headers.Add("format", "pdf");
headers.Add("shape2", "a4");

var properties = channel.CreateBasicProperties();
properties.Headers = headers;
properties.Persistent = true;

var messageBody = Encoding.UTF8.GetBytes("Header mesajım...");
channel.BasicPublish("header-exchange", "", properties, messageBody);

Console.WriteLine("Mesaj gönderildi.");

Console.ReadLine();

enum LogTypes
{
    Critical = 1,
    Error,
    Warning,
    Info
}
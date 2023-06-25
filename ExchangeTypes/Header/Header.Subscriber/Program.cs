using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://xzualgjx:JpisQbL7GEymibg7IGJqB7nU05-50RvZ@armadillo.rmq.cloudamqp.com/xzualgjx");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

/*
 -> prefetchSize parametresi : Mesaj boyutunu ifade eder. 0(sıfır) diyerek ilgilenmediğimizi belirtiyoruz.
 -> prefetchCount parametresi : Dağıtım adetini ifade eder.
 -> global parametresi : true ise, tüm consumerların aynı anda prefetchCount parametresinde belirtilen değer kadar mesaj tüketebileceğini ifade eder. false değeri ise, her bir consumerın bir işleme süresinde diğer consumerlardan bağımsız bir şekilde kaç mesaj alıp işleyeceğini belirtir.
*/
channel.BasicQos(0, 1, false);

var consumer = new EventingBasicConsumer(channel);

channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

var queueName = channel.QueueDeclare().QueueName;

Dictionary<string, object> headers = new Dictionary<string, object>();
headers.Add("format", "pdf");
headers.Add("shape", "a4");
headers.Add("x-match", "any"); // bu subscriber tarafında mutlaka eklenmelidir. "all" ise tüm keylerin varlığını kontrol eder ve olması gerekllidir. "any" ise herhangi biri eşleşirse mesajı işler.

channel.QueueBind(queueName, "header-exchange", "", headers);

Console.WriteLine($"Loglar dinleniyor. Kuyruk adı: {queueName}");

consumer.Received += (sender, args) =>
{
    var message = Encoding.UTF8.GetString(args.Body.ToArray());
    Thread.Sleep(1500);
    Console.WriteLine($"Gelen mesaj: {message}");
    // kuyruktan mesajın artık silinebileceği rabbitmq'ya bildiriliyor. multiple => false ise sadece kendi mesajın durumunu bildirir. true ise diğerlerinin durumlarını da bildirir.
    channel.BasicAck(args.DeliveryTag, false);
};

channel.BasicConsume(queueName, false, consumer);

Console.ReadLine();
﻿using RabbitMQ.Client;

namespace ExcelCreate.Worker.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQClientService> _logger;
        private IConnection _connection;
        private IModel _channel;
        public static string QueueName = "queue-excel-file";

        public RabbitMQClientService(
            ConnectionFactory connectionFactory,
            ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connectionFactory.DispatchConsumersAsync = true;

            _connection = _connectionFactory.CreateConnection();

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu.");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ ile bağlantı kesildi.");
        }
    }

}

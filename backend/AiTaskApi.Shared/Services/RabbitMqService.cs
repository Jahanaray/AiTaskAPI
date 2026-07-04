using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace AiTaskApi.Shared.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync
                    (
                        queue: "agent-jobs",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    ).GetAwaiter().GetResult();
        }

        public void Publish(string queueName, string message)
        {

        }

        public async Task PublishAsync<T>(string queue, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queue,
                mandatory: false,
                body: body);
        }



    }


}

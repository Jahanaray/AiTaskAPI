using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiTaskApi.Worker.Services
{
    public class RabbitMqConsumer
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;


        public RabbitMqConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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


        public async Task StartAsync(CancellationToken token)
        {
        }

    }
}

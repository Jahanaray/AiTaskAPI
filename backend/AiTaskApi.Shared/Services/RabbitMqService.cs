using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.Client;

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
                HostName = "localhost"
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
    }


}

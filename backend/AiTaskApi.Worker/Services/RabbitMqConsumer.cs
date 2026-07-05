using AiTaskApi.Shared.Models.Agent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var jobId = JsonSerializer.Deserialize<int>(json);
                //var job = JsonSerializer.Deserialize<AgentJob>(json);

                if (jobId == null)
                {
                    await _channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var processor = scope.ServiceProvider.GetRequiredService<JobProcessorService>();

                await processor.RunJob(jobId, token);
                //await processor.RunJob(job, token);
                
                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false);
            };

            await _channel.BasicConsumeAsync(
                queue: "agent-jobs",
                autoAck: false,
                consumer: consumer,
                cancellationToken: token);
        }

    }
}

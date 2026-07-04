using System;
using System.Collections.Generic;
using System.Text;

namespace AiTaskApi.Shared.Services
{
    public interface IRabbitMqService
    {
        void Publish(string queueName, string message);
    }
}

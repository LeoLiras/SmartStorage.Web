using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartStorage.Configurations.Config.RabbitMQ;
using SmartStorage.EmailAPI.Repository;
using SmartStorage.EmailAPI.Repository.Interfaces;
using SmartStorage_Shared.VO;
using System.Text;
using System.Text.Json;

namespace SmartStorage.EmailAPI.MessageConsumer
{
    public class RabbitMQEmailConsumer : BackgroundService
    {
        private readonly IEmailRepository _repository;
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQEmailConsumer> _logger;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQEmailConsumer(IEmailRepository repository, RabbitMQSettings settings, ILogger<RabbitMQEmailConsumer> logger)
        {
            _repository = repository;
            _settings = settings;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "sendemailqueue", false, false, false, arguments: null);
            _channel.QueueDeclare(queue: LowStockAlertVO.QueueName, false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                ProductVO vo = JsonSerializer.Deserialize<ProductVO>(content);
                ProcessOrder(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("sendemailqueue", false, consumer);

            var lowStockConsumer = new EventingBasicConsumer(_channel);
            lowStockConsumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                var alert = JsonSerializer.Deserialize<LowStockAlertVO>(content);

                try
                {
                    _repository.LowStockEmail(alert).GetAwaiter().GetResult();
                    _channel.BasicAck(evt.DeliveryTag, false);

                    _logger.LogInformation("E-mail de estoque mínimo enviado para o produto {ProductId}.", alert.ProductId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao enviar o e-mail de estoque mínimo do produto {ProductId}.", alert.ProductId);
                }
            };
            _channel.BasicConsume(LowStockAlertVO.QueueName, false, lowStockConsumer);

            return Task.CompletedTask;
        }

        private async Task ProcessOrder(ProductVO vo)
        {
            await _repository.NewProductEmail(vo);
        }
    }
}

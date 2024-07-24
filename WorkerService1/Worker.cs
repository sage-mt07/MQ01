using MQLib;
namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Messaging _messagingLibrary;

        public Worker(ILogger<Worker> logger,Messaging message)
        {
            _logger = logger;
            _messagingLibrary= message;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Example: Send a message
                    _messagingLibrary.SendMessage("Hello, MQ!");

                    // Example: Receive a message
                    string receivedMessage = _messagingLibrary.ReceiveMessage();
                    _logger.LogInformation("Received message: {message}", receivedMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing MQ messages.");
                }

                await Task.Delay(1000, stoppingToken); // Adjust the delay as needed
            }
        }
    }
}

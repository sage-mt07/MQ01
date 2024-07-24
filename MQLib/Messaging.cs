using IBM.WMQ;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQLib
{
    public class Messaging
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ILogger<Messaging> _logger;

        public Messaging(Logger<Messaging> logger,ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public void SendMessage(string message)
        {
            ExecuteWithRetry((queue, queueManager) =>
            {
                var mqMessage = new MQMessage();
                mqMessage.WriteString(message);
                var mqPutMessageOptions = new MQPutMessageOptions();
                queue.Put(mqMessage, mqPutMessageOptions);
                var config = _connectionManager.CurrentConfiguration;
                _logger.LogInformation($"Message received successfully from {config.Host}:{config.Port}");

                LogServerInfo(_connectionManager.CurrentHost, _connectionManager.CurrentPort);
            });
        }

        public string ReceiveMessage()
        {
            return ExecuteWithRetry((queue, queueManager) =>
            {
                var mqMessage = new MQMessage();
                queue.Get(mqMessage);
                var config = _connectionManager.CurrentConfiguration;
                _logger.LogInformation($"Message received successfully from {config.Host}:{config.Port}");

                return mqMessage.ReadString(mqMessage.DataLength);
            });
        }

        private void ExecuteWithRetry(Action<MQQueue, MQQueueManager> action)
        {
            _connectionManager.ResetServerIndex();

            while (true)
            {
                MQQueueManager queueManager = null;
                MQQueue queue = null;

                try
                {
                    queueManager = _connectionManager.GetQueueManager();
                    var config = _connectionManager.CurrentConfiguration;
                    queue = queueManager.AccessQueue(config.Queue, MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_AS_Q_DEF);

                    action(queue, queueManager);
                    break;
                }
                catch (MQException ex)
                {
                    Console.WriteLine($"Failed to perform operation on {_connectionManager.CurrentConfiguration.Host}:{_connectionManager.CurrentConfiguration.Port} - {ex.Message}");

                    if (_connectionManager.IsLastConfiguration())
                    {
                        throw new Exception("Unable to perform operation on any MQ server.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error occurred: {ex.Message}");
                    throw;
                }
                finally
                {
                    queue?.Close();
                    queueManager?.Disconnect();
                }
            }
        }

        private T ExecuteWithRetry<T>(Func<MQQueue, MQQueueManager, T> action)
        {
            _connectionManager.ResetServerIndex();

            while (true)
            {
                MQQueueManager queueManager = null;
                MQQueue queue = null;

                try
                {
                    queueManager = _connectionManager.GetQueueManager();
                    var config = _connectionManager.CurrentConfiguration;
                    queue = queueManager.AccessQueue(config.Queue, MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_AS_Q_DEF);

                    return action(queue, queueManager);
                }
                catch (MQException ex)
                {
                    Console.WriteLine($"Failed to perform operation on {_connectionManager.CurrentConfiguration.Host}:{_connectionManager.CurrentConfiguration.Port} - {ex.Message}");

                    if (_connectionManager.IsLastConfiguration())
                    {
                        throw new Exception("Unable to perform operation on any MQ server.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error occurred: {ex.Message}");
                    throw;
                }
                finally
                {
                    queue?.Close();
                    queueManager?.Disconnect();
                }
            }
        }

        private void LogServerInfo(string host, int port)
        {
            Console.WriteLine($"Operation performed on server {host} on port {port}");
        }
    }
}

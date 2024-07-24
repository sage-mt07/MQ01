using IBM.WMQ;
using System.Collections;

namespace MQLib
{
    public class ConnectionManager
    {
        private readonly MQConfiguration _mqConfiguration;
        private int _currentHostIndex = 0;
        private int _currentPortIndex = 0;
        private MQQueueManager _currentQueueManager;

        public ConnectionManager(MQConfiguration mqConfiguration)
        {
            _mqConfiguration = mqConfiguration;
        }

        public MQQueueManager GetQueueManager()
        {
            if (_currentQueueManager != null && _currentQueueManager.IsConnected)
            {
                return _currentQueueManager;
            }

            _currentQueueManager = null;

            while (_currentHostIndex < _mqConfiguration.Hosts.Count)
            {
                var hostConfig = _mqConfiguration.Hosts[_currentHostIndex];
                while (_currentPortIndex < hostConfig.Ports.Count)
                {
                    var port = hostConfig.Ports[_currentPortIndex];
                    try
                    {
                        var properties = new Hashtable
                    {
                        { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_CLIENT },
                        { MQC.HOST_NAME_PROPERTY, hostConfig.Host },
                        { MQC.PORT_PROPERTY, port },
                        { MQC.CHANNEL_PROPERTY, _mqConfiguration.Channel }
                    };

                        _currentQueueManager = new MQQueueManager(_mqConfiguration.QueueManager, properties);
                        return _currentQueueManager;
                    }
                    catch (MQException)
                    {
                        _currentPortIndex++;
                    }
                }
                _currentHostIndex++;
                _currentPortIndex = 0;
            }

            throw new Exception("Unable to connect to any MQ server.");
        }

        public void ResetServerIndex()
        {
            _currentHostIndex = 0;
            _currentPortIndex = 0;
        }
        public (string Host, int Port, string Queue) CurrentConfiguration
        {
            get
            {
                if (_currentHostIndex < _mqConfiguration.Hosts.Count && _currentPortIndex < _mqConfiguration.Hosts[_currentHostIndex].Ports.Count)
                {
                    return (_mqConfiguration.Hosts[_currentHostIndex].Host, _mqConfiguration.Hosts[_currentHostIndex].Ports[_currentPortIndex], _mqConfiguration.Queue);
                }
                return (null, 0, null);
            }
        }
        public bool IsLastConfiguration()
        {
            return _currentHostIndex == _mqConfiguration.Hosts.Count - 1 && _currentPortIndex == _mqConfiguration.Hosts.Last().Ports.Count - 1;
        }
        public string CurrentHost => _mqConfiguration.Hosts[_currentHostIndex].Host;
        public int CurrentPort => _mqConfiguration.Hosts[_currentHostIndex].Ports[_currentPortIndex];
    }
}

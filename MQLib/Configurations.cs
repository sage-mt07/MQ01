namespace MQLib
{
    public class MQHostConfiguration
    {
        public string Host { get; set; }
        public List<int> Ports { get; set; }
    }

    public class MQConfiguration
    {
        public List<MQHostConfiguration> Hosts { get; set; }
        public string QueueManager { get; set; }
        public string Channel { get; set; }
        public string Queue { get; set; }
    }

}

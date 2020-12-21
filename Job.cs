using System;

namespace RabbitMQTest
{
    public class Job
    {
        public int Id { get; set; }
        public int Entry { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string ExecuteAsName { get; set; }
        public string ExecutingAgentName { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime StartTime { get; set; }
    }
}
using RabbitMQ.Client;
using System;
using System.Globalization;
using System.Text;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace RabbitMQTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            
        }

        /// <summary>
        /// The Consumer
        /// </summary>
        private static void ConsumeMessage()
        {
            // creates connection factory with given host name
            var factory = new ConnectionFactory { HostName = "localhost" };

            // open a connection 
            using var connection = factory.CreateConnection();

            // open a channel
            using var channel = connection.CreateModel();

            // declare queue 
            channel.QueueDeclare("jobQ1", false, false, false, null);
            channel.QueueDeclare("jobQ2", false, false, false, null);

            var consumer1 = new EventingBasicConsumer(channel);
            consumer1.Received += Consumer_Received;
            channel.BasicConsume(consumer1, "jobQ1");

            var consumer2 = new EventingBasicConsumer(channel);
            consumer2.Received += Consumer_Received;
            channel.BasicConsume(consumer2, "jobQ2");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Process received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            var job = JsonConvert.DeserializeObject<Job>(content);
            Console.WriteLine("_____________________");
            Console.WriteLine($" [x] Received  from exchange = {e.Exchange}, RoutingKey = {e.RoutingKey}");
            Console.WriteLine($" [x] Received Job id = {job.Id}, JAMS entry = {job.Entry}, name = {job.Name}, status = {job.Status}, start time = {job.StartTime}, submitted by {job.SubmittedBy}");
        }

        /// <summary>
        /// The producer 
        /// </summary>
        /// <param name="kj">job</param>
        private static void SendMessage(Job kj)
        {
            // creates connection factory with given host name
            var factory = new ConnectionFactory { HostName = "localhost" };

            // open a connection 
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            // declare queue 
            channel.QueueDeclare("jobQ1", false, false, false, null);

            // declare queue 
            channel.QueueDeclare("jobQ2", false, false, false, null);

            // declare exchange
            channel.ExchangeDeclare("demoExchange", ExchangeType.Direct);

            // declare exchange
            channel.ExchangeDeclare("toAllExchange", ExchangeType.Fanout);

            // Bind Queue to Exchange
            channel.QueueBind("jobQ1", "demoExchange", "completion_key");

            // Bind Queue to Exchange
            channel.QueueBind("jobQ1", "toAllExchange", "");
            channel.QueueBind("jobQ2", "toAllExchange", "");

            // creates send job obj
            var jobObj = new
            {
                Id = kj.Id,
                JamsEntry = kj.Entry,
                Name = kj.Name,
                Status = kj.Status,
                ExecuteAsName = kj.ExecuteAsName,
                ExecutingAgentName = kj.ExecutingAgentName,
                SubmittedBy = kj.SubmittedBy?.Replace("\\", "."),
                StartTime = kj.StartTime.ToString(CultureInfo.InvariantCulture)
            };

            // convert model as a message body
            var json = JsonConvert.SerializeObject(jobObj);
            var body = Encoding.UTF8.GetBytes(json);

            // publish 
            channel.BasicPublish("demoExchange", "completion_key", null, body);
            channel.BasicPublish("toAllExchange", "", null, body);

            // close the channel
            channel.Close();
            // close the connection
            connection.Close();
        }
    }
}

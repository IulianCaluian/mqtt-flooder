using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttFlooder
{
    static internal class MqttFlooder
    {

        public static void StartTheFlood(IConfiguration configuration, FloodOptions options)
        {


            // Start an MQQT client:
            string sClientId, sTcpServer, sUserName, sPassword, sPublishTopicResponses, sPublishTopicEvents;

            sClientId = configuration["MQTT:ClientId"];
            sTcpServer = configuration["MQTT:TcpServer"];

            sUserName = configuration["MQTT:User"];
            sPassword = configuration["MQTT:Pass"];
            




            Console.WriteLine($"Starting the flooding of server [{sTcpServer}].");


            Console.WriteLine("Flooding ended.");
        
        }
    }
}

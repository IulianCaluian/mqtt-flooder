using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MqttFlooder
{
    static internal class MqttFlooder
    {

        public async static Task StartTheFlood(IConfiguration configuration, FloodOptions options)
        {


            // Start an MQQT client:
            string sClientId, sTcpServer, sUserName, sPassword, sPublishTopicResponses, sPublishTopicEvents;

            sClientId = configuration["MQTT:ClientId"];
            sTcpServer = configuration["MQTT:TcpServer"];

            sUserName = configuration["MQTT:User"];
            sPassword = configuration["MQTT:Pass"];
            




            Console.WriteLine($"Starting the flooding of server [{sTcpServer}].");

            // Create a new MQTT client.
            var factory = new MqttFactory();
            IMqttClient _mqttClient = factory.CreateMqttClient();


            // Create TCP based options using the builder.
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(sClientId + "Sender")
                .WithTcpServer(sTcpServer);

            if (string.IsNullOrEmpty(sUserName) == false)
                optionsBuilder = optionsBuilder.WithCredentials(sUserName, sPassword);

            optionsBuilder = optionsBuilder.WithCleanSession();

            var mqtt_options = optionsBuilder.Build();

            // Setting handlers:

            _mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;

            await _mqttClient.ConnectAsync(mqtt_options); 

            
            
            
            await SendMessage(_mqttClient, options.Topic);

   
       




            try
            {
                // 5. Cleanup:
                Console.WriteLine("Disconect async");
                await _mqttClient.DisconnectAsync();

                // 6. Dispose:
                Console.WriteLine("Dispose");
                _mqttClient.Dispose();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Flooding ended.");
        
        }

        private async static Task SendMessage(IMqttClient mqttClient, string publisingTopic)
        {
            JsonObject obj = new JsonObject();
            obj.Add("prop", "test");

            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();

            string json_msg_to_send = JsonSerializer.Serialize(obj);
            byte[] binaryPayload = Encoding.UTF8.GetBytes(json_msg_to_send);

            try
            {
                Console.WriteLine($"info, try to publish on topic: {publisingTopic}/{myuuidAsString}");

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{publisingTopic}/{myuuidAsString}")
                    .WithPayload(binaryPayload)
                //??   .WithExactlyOnceQoS()
                //??    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(message); // Since 3.0.5 with CancellationToken
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at sending on bridge topic:{ex.Message}");
            }


        }

        private static Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");
            return Task.CompletedTask;
        }

        private static Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg) {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            return Task.CompletedTask;

        } 

    }


}

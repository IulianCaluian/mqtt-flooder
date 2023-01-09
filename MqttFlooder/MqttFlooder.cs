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
            IMqttClient mqttClient = factory.CreateMqttClient();


            // Create TCP based options using the builder.
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(sClientId + "Sender")
                .WithTcpServer(sTcpServer);

            if (string.IsNullOrEmpty(sUserName) == false)
                optionsBuilder = optionsBuilder.WithCredentials(sUserName, sPassword);

            optionsBuilder = optionsBuilder.WithCleanSession();

            var mqtt_options = optionsBuilder.Build();

            // Setting handlers:

            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;

            await mqttClient.ConnectAsync(mqtt_options);

            await StartSendingRequests(mqttClient, options);
       
            try
            {
                // 5. Cleanup:
                Console.WriteLine("Disconect async");
                await mqttClient.DisconnectAsync();

                // 6. Dispose:
                Console.WriteLine("Dispose");
                mqttClient.Dispose();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Flooding ended.");
        
        }

        private async static Task StartSendingRequests(IMqttClient mqttClient ,FloodOptions options)
        {
            if (options.Topic is null)
                throw new ArgumentException("The topic for mqtt is missing.");

            int nrOfRequestSent = options.RequestsCount;
            if (nrOfRequestSent <= 0) 
                nrOfRequestSent = 1;

            int delay = options.RequestsInterval;
            if (delay < 0) 
                delay = 0;

            JsonObject? obj = null;
            if (options.ContentFilename != null)
            {
                string txt = File.ReadAllText(options.ContentFilename);
                obj = JsonSerializer.Deserialize<JsonObject>(txt);


            }

            for (int i = 0; i < nrOfRequestSent; i++)
            {
                await SendMessage(mqttClient, options.Topic, obj);

                await Task.Delay(delay);
            }
        }

        private async static Task SendMessage(IMqttClient mqttClient, string publisingTopic, JsonObject? payload)
        {
   
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();

            string json_msg_to_send = JsonSerializer.Serialize(payload);
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

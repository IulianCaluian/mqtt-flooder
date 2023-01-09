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
            Console.WriteLine($"Starting the flooding of server.");

            // Create a new MQTT client.
            var factory = new MqttFactory();
            IMqttClient mqttClient = factory.CreateMqttClient();

            // Create and apply the MQTT client options.
            var mqttOptions = BuildMqttOptions(configuration);
            AssignMqttEventHandlers(mqttClient);

            bool connectionSuccess = await ConnectMqttClientAsync(mqttClient, mqttOptions);

            if (connectionSuccess)
            {
                await StartSendingRequests(mqttClient, options);
            }

            await DisconnectMqttClientAsync(mqttClient);
            mqttClient.Dispose();


            Console.WriteLine("Flooding ended.");
        
        }

        private static MqttClientOptions BuildMqttOptions(IConfiguration configuration)
        {
            // Extract the MQTT client configuration values.
            string sClientId = configuration["MQTT:ClientId"];
            string sTcpServer = configuration["MQTT:TcpServer"];
            string sUserName = configuration["MQTT:User"];
            string sPassword = configuration["MQTT:Pass"];

            // Create the MQTT client options builder.
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(sClientId + "Sender")
                .WithTcpServer(sTcpServer);

            // Set the username and password if they are specified.
            if (string.IsNullOrEmpty(sUserName) == false)
                optionsBuilder = optionsBuilder.WithCredentials(sUserName, sPassword);

            // Set the clean session flag.
            optionsBuilder = optionsBuilder.WithCleanSession();

            // Build the MQTT client options.
            return optionsBuilder.Build();
        }

        private static void AssignMqttEventHandlers(IMqttClient mqttClient)
        {
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
        }

        private async static Task StartSendingRequests(IMqttClient mqttClient ,FloodOptions options)
        {
            if (options.Topic is null)
                throw new ArgumentException("The topic for mqtt is missing.");

            // Read the JSON content from the file, if specified.
            JsonObject? jsonContent = ReadJsonContentFromFile(options);

            // Set default values for the number of requests and the interval between requests.
            int nrOfRequestSent = options.RequestsCount > 0 ? options.RequestsCount : 1;
            int delay = options.RequestsInterval >= 0 ? options.RequestsInterval : 0;

            foreach (int i in Enumerable.Range(0, nrOfRequestSent))
            {
                await SendMessageAsync(mqttClient, options.Topic, jsonContent);
                await Task.Delay(delay);
            }
        }

        private static JsonObject? ReadJsonContentFromFile(FloodOptions options)
        {
            if (options.ContentFilename is null)
                return null;

            string jsonString = File.ReadAllText(options.ContentFilename);
            return JsonSerializer.Deserialize<JsonObject>(jsonString);
        }


        private async static Task SendMessageAsync(IMqttClient mqttClient, string publisingTopic, JsonObject? payload)
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

        private static async Task<bool> ConnectMqttClientAsync(IMqttClient mqttClient, MqttClientOptions mqttOptions)
        {
            try
            {
                Console.WriteLine("Connecting to MQTT server...");
                await mqttClient.ConnectAsync(mqttOptions);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MQTT server: {ex}");
                return false;
            }
        }

        private static async Task DisconnectMqttClientAsync(IMqttClient mqttClient)
        {
            try
            {
                Console.WriteLine("Disconnecting from MQTT server...");
                await mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from MQTT server: {ex}");
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

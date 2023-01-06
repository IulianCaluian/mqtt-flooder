using CommandLine;
using Microsoft.Extensions.Configuration;
using MqttFlooder;

namespace MqttFlooder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false)
                 .Build();

            Parser.Default.ParseArguments<FloodOptions>(args)
                .WithParsed<FloodOptions>(o =>
                {
                    MqttFlooder.StartTheFlood(config, o);
                });
        }
    }
}
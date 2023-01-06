using CommandLine;
using Microsoft.Extensions.Configuration;
using MqttFlooder;

namespace MqttFlooder
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false)
                 .Build();

           await Parser.Default.ParseArguments<FloodOptions>(args)
               .WithParsedAsync(async o =>
               {
                    try
                    {
                        await MqttFlooder.StartTheFlood(config, o);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
               });


            Console.WriteLine("done");


        }


    }
}
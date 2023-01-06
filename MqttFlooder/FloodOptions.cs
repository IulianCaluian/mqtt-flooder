using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttFlooder
{
    
    public class FloodOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("publish-topic", Required = true, HelpText = "Topic where the requests will be published.")]
        public string? Topic { get; set; }

        [Option("content-filename", Required = false, HelpText = "Input filename for the request's content.")]
        public string? ContentFilename { get; set; }

        [Option("count", Required = false, HelpText = "The number of requests to be made.")]
        public int RequestsCount { get; set; }

        [Option("interval", Required = false, HelpText = "The interval between requests, in milliseconds.")]
        public int RequestsRate { get; set; }

        [Usage(ApplicationAlias = "mqttflooder")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal scenario", new FloodOptions { Topic = "io/commands", ContentFilename = "message1.json" });
                yield return new Example("Sending 100 messages", new FloodOptions { Topic = "io/commands", ContentFilename = "message2.json", RequestsCount = 100 });
                yield return new Example("Rate-limiting at 10ms intervals", new FloodOptions { RequestsRate = 10 });
            }
        }

    }
}

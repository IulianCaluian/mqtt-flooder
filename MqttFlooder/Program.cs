using CommandLine;
using CommandLine.Text;

Parser.Default.ParseArguments<Options, FloodOptions>(args)
    .WithParsed<Options>(o =>
    {
        if (o.Verbose)
        {
            Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
            Console.WriteLine("Quick Start Example! App is in Verbose mode!");
        }
        else
        {
            Console.WriteLine($"Current Arguments: -v {o.Verbose}");
            Console.WriteLine("Quick Start Example!");
        }
    })
    .WithParsed<FloodOptions>(fo =>
    {
        //TODO.
    });


public class Options {

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
}

[Verb("flood", HelpText = "Send a number of requests to a topic.")]
public class FloodOptions
{
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
            yield return new Example("Normal scenario", new FloodOptions { Topic = "io/commands", ContentFilename="message1.json" });
            yield return new Example("Sending 100 messages", UnParserSettings.WithGroupSwitchesOnly(), new FloodOptions { Topic = "io/commands", ContentFilename = "message2.json", RequestsCount = 100 });
            yield return new Example("Rate-limiting at 10ms intervals", new[] { UnParserSettings.WithGroupSwitchesOnly(), UnParserSettings.WithUseEqualTokenOnly() }, new FloodOptions { RequestsRate = 10 });
        }
    }

}
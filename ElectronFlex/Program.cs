using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using WatsonWebserver;

namespace ElectronFlex
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    Config.CommandLineOptions = o;
                });
            
            Console.WriteLine($"wp={Config.CommandLineOptions.WebPort}, wsp={Config.CommandLineOptions.WebSocketPort}");
            var s = new Server("127.0.0.1", Config.CommandLineOptions.WebPort, false, DefaultRoute);

            // add content routes
            s.Routes.Content.Add("/wwwroot/", true);

            // start the server
            s.Start();
            Console.WriteLine($"Web Server Started => 127.0.0.1:{Config.CommandLineOptions.WebPort}");
            
            Console.SetOut(new ElectronTextWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            });
            Console.WriteLine("Hello World!");
            NodeJs.WriteLine("Hello");
            Console.Write("$$$console.log('direct call from cs');\n");

            var line="";
            do
            {
                line = Console.ReadLine();
                Console.WriteLine($"NodeJs: {line}");
            } while (line != "exit");
        }
        
        static async Task DefaultRoute(HttpContext ctx)
        { 
            await ctx.Response.Send("No content provided by this route");
        }
    }

    
}

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using WatsonWebserver;
using WatsonWebsocket;

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
            Console.SetOut(new ElectronTextWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            });
            
            Console.WriteLine($"wp={Config.CommandLineOptions.WebPort}, wsp={Config.CommandLineOptions.WebSocketPort}");
            StartWebServer();
            StartWebSocket();


            //Test
            Console.WriteLine("Hello World!");
            NodeJs.WriteLine("Hello");
            NodeJs.Invoke("console.log('direct call from cs');");
            
            NodeJs.Loop();
        }

        private static void StartWebSocket()
        {
            WatsonWsServer server = new WatsonWsServer("127.0.0.1", Config.CommandLineOptions.WebSocketPort);
            server.MessageReceived += MessageReceived; 
            server.Start();
            Console.WriteLine($"Web Socket Started => 127.0.0.1:{Config.CommandLineOptions.WebSocketPort}");
            
            Config.WebSocketServer = server;
        }

        private static void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            var stream = Config.WebSocketStream;
            stream.WriteBytes(e.Data);

            while (stream.HasSizeForRead(sizeof(int)))
            {
                var size = stream.ReadInt32();
                if (!stream.HasSizeForRead(size))
                {
                    stream.UnReadInt32();
                    break;
                }

                var packBuff = stream.ReadBytes(size);
                var pack = NodePack.Decode(packBuff);
                Console.WriteLine($"ws recv: {Newtonsoft.Json.JsonConvert.SerializeObject(pack)}");
                pack.Content = $"srv: {pack.Content}";
                Config.WebSocketServer.SendAsync(e.IpPort, pack.Encode());
            }
        }

        private static void StartWebServer()
        {
            var s = new Server("127.0.0.1", Config.CommandLineOptions.WebPort, false, DefaultRoute);

            // add content routes
            s.Routes.Content.Add("/wwwroot/", true);

            // start the server
            s.Start();
            Console.WriteLine($"Web Server Started => 127.0.0.1:{Config.CommandLineOptions.WebPort}");

            Config.WebServer = s;
        }

        static async Task DefaultRoute(HttpContext ctx)
        { 
            await ctx.Response.Send("No content provided by this route");
        }
    }

    
}

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

            Task.Run(BrowserJs.Loop);
            NodeJs.Loop();
        }

        private static void StartWebServer()
        {
            var s = new Server("127.0.0.1", Config.CommandLineOptions.WebPort, false, WebServerHandler.DefaultRoute);

            // add content routes
            s.Routes.Content.Add("/wwwroot/", true);

            // start the server
            s.Start();
            Console.WriteLine($"Web Server Started => 127.0.0.1:{Config.CommandLineOptions.WebPort}");

            Config.WebServer = s;
        }

        private static void StartWebSocket()
        {
            WatsonWsServer server = new WatsonWsServer("127.0.0.1", Config.CommandLineOptions.WebSocketPort);
            server.ClientConnected += WebSocketHandler.ClientConnected;
            server.ClientDisconnected += WebSocketHandler.ClientDisconnected;
            server.MessageReceived += WebSocketHandler.MessageReceived;
            server.Start();
            Console.WriteLine($"Web Socket Started => 127.0.0.1:{Config.CommandLineOptions.WebSocketPort}");
            
            Config.WebSocketServer = server;
        }
    }
}

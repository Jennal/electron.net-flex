using WatsonWebserver;
using WatsonWebsocket;
using System.IO;

namespace ElectronFlex
{
    public static class Config {
        public static CommandLineOptions CommandLineOptions;
        
        public static Server WebServer;
        public static WatsonWsServer WebSocketServer;
        
        public static WebSocketStream WebSocketStream = new WebSocketStream();

        public static string WebSocketClientIpPort;
    }
}
using System;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebsocket;

namespace ElectronFlex
{
    public static class WebSocketHandler
    {
        public static void ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            // Task.Run(() =>
            // {
            //     var task = BrowserJs.Invoke<int>("Math.abs(-1)");
            //     task.Wait();
            //     Console.WriteLine($"js result: {task.Result}");
            // });
        }

        public static void ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
        }

        public static void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            var stream = Config.WebSocketStream;
            stream.WriteBytes(e.Data);
            // Console.WriteLine($"Recv: {e.Data.ToJson()}");
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ElectronFlex
{
    public class BrowserInvoke
    {
        public string Class;
        public string Method;
        public object[] Arguments;
    }
    
    public static class BrowserJs
    {
        private static IdGenerator s_idGenerator = new IdGenerator();
        private static InvokeTaskManager s_taskManager = new InvokeTaskManager();
        
        public static Task Invoke(string jsCode)
        {
            return Invoke<IgnoreReturn>(jsCode);
        }
        
        public static Task<T> Invoke<T>(string jsCode)
        {
            var pack = new NodePack
            {
                Id = s_idGenerator.Next(),
                Type = NodePackType.InvokeCode,
                Content = jsCode
            };
            var task = s_taskManager.Invoke<T>(pack);
            Send(pack);

            return task;
        }

        public static void Loop()
        {
            var stream = Config.WebSocketStream;
            while (true)
            {
                if (!stream.HasSizeForRead(sizeof(int)))
                {
                    Thread.Sleep(100);
                    continue;
                }
                
                var size = stream.ReadInt32();
                if (!stream.HasSizeForRead(size))
                {
                    stream.UnReadInt32();
                    Thread.Sleep(100);
                    continue;
                }

                var packBuff = stream.ReadBytes(size);
                var pack = NodePack.Decode(packBuff);
                Console.WriteLine($">>>>>>> WebSocket.Recv: {pack}");

                switch (pack.Type)
                {
                    case NodePackType.InvokeCode:
                        var invoke = JsonConvert.DeserializeObject<BrowserInvoke>(pack.Content);
                        //TODO: find class & method & invoke
                        
                        pack.Type = NodePackType.InvokeResult;
                        Send(pack);
                        break;
                    case NodePackType.InvokeResult:
                        s_taskManager.Result(pack);
                        break;
                }
            }
        }
        
        public static void WriteLine(string? line)
        {
            line = line?.TrimEnd('\n');
            line = line?.TrimEnd('\r');
            var pack = new NodePack
            {
                Id = s_idGenerator.Next(),
                Type = NodePackType.ConsoleOutput,
                Content = line
            };

            Send(pack);
        }

        private static void Send(NodePack pack)
        {
            Console.WriteLine($"<<<<<<< WebSocket.Send: {pack}");
            var ipPort = Config.WebSocketServer.ListClients().FirstOrDefault();
            Config.WebSocketServer.SendAsync(ipPort, pack.Encode());
        }
    }
}
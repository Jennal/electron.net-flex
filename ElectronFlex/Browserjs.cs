using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElectronFlex
{
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
            Send(pack);

            return s_taskManager.Invoke<T>(pack);
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
                Console.WriteLine($"ws recv: {Newtonsoft.Json.JsonConvert.SerializeObject(pack)}");

                switch (pack.Type)
                {
                    case NodePackType.InvokeCode:
                        //TODO:
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
            var ipPort = Config.WebSocketServer.ListClients().FirstOrDefault();
            Config.WebSocketServer.SendAsync(ipPort, pack.Encode());
        }
    }
}
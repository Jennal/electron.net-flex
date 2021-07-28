using System;
using System.Linq;
using System.Reflection;
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
            var pack = new Pack
            {
                Id = s_idGenerator.Next(),
                Type = PackType.InvokeCode,
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
                    Thread.Sleep(20);
                    continue;
                }
                
                var size = stream.ReadInt32();
                if (!stream.HasSizeForRead(size))
                {
                    stream.UnReadInt32();
                    Thread.Sleep(20);
                    continue;
                }

                var packBuff = stream.ReadBytes(size);
                var pack = Pack.Decode(packBuff);
                Console.WriteLine($">>>>>>> WebSocket.Recv: {pack}");

                switch (pack.Type)
                {
                    case PackType.InvokeCode:
                        InvokeFromBrowser(pack);
                        break;
                    case PackType.InvokeResult:
                        s_taskManager.Result(pack);
                        break;
                }
            }
        }

        private static void InvokeFromBrowser(Pack pack)
        {
            var invoke = JsonConvert.DeserializeObject<BrowserInvoke>(pack.Content);
            var (ns, _) = SplitNsClass(invoke.Class);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (!string.IsNullOrEmpty(ns))
                assemblies = assemblies.Where(o => o.GetName().Name.StartsWith(ns)).ToArray();
            // else assemblies = new[] {typeof(BrowserInvoke).Assembly};

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(invoke.Class);
                if (type == null) continue;

                var method = type.GetMethod(invoke.Method, BindingFlags.Static | BindingFlags.Public, invoke.Arguments);
                var result = method.Invoke(null, invoke.Arguments);
                Send(new Pack
                {
                    Id = pack.Id,
                    Type = PackType.InvokeResult,
                    Content = JsonConvert.SerializeObject(result)
                });
                return;
            }
            
            Send(new Pack
            {
                Id = pack.Id,
                Type = PackType.InvokeResult,
                Content = InvokeError.Error(invoke, $"can't find method")
            });
        }

        private static Tuple<string, string> SplitNsClass(string invokeClass)
        {
            if (string.IsNullOrEmpty(invokeClass)) return null;
            
            var idx = invokeClass.LastIndexOf(".");
            return new Tuple<string, string>(
                invokeClass.Substring(0, idx), 
                invokeClass.Substring(idx + 1, invokeClass.Length - idx - 1)
            );
        }

        public static void WriteLine(string? line)
        {
            line = line?.TrimEnd('\n');
            line = line?.TrimEnd('\r');
            var pack = new Pack
            {
                Id = s_idGenerator.Next(),
                Type = PackType.ConsoleOutput,
                Content = line
            };

            Send(pack);
        }

        private static void Send(Pack pack)
        {
            Console.WriteLine($"<<<<<<< WebSocket.Send: {pack}");
            var ipPort = Config.WebSocketServer.ListClients().FirstOrDefault();
            Config.WebSocketServer.SendAsync(ipPort, pack.Encode());
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ElectronFlex
{
    public enum NodePackType : byte
    {
        ConsoleOutput = 0,
        InvokeCode = 1,
        InvokeResult = 2,
    }
    
    public struct NodePack
    {
        public byte Id;
        public NodePackType Type;
        public string Content;

        public byte[] Encode()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            
            var len = 6 + Content.Length;
            bw.Write((int)len);
            bw.Write((byte)Id);
            bw.Write((byte)Type);
            bw.Write((int)Content.Length);
            bw.Write(Encoding.UTF8.GetBytes(Content));
            bw.Flush();

            return ms.ToArray();
        }

        public static NodePack Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var pack = new NodePack
            {
                Id = br.ReadByte(),
                Type = (NodePackType)br.ReadByte(),
            };

            var length = br.ReadInt32();
            var content = br.ReadBytes(length);
            pack.Content = Encoding.UTF8.GetString(content);
            
            return pack;
        }
    }

    class IgnoreReturn
    {}

    public static class NodeJs
    {
        private static IdGenerator s_idGenerator = new IdGenerator();
        public static ConcurrentDictionary<byte, object> s_dict = new ConcurrentDictionary<byte, object>();
        
        public static Task Invoke(string jsCode)
        {
            return Invoke<IgnoreReturn>(jsCode);
        }
        
        public static Task<T> Invoke<T>(string jsCode)
        {
            if (!Config.CommandLineOptions.StartFromElectron)
            {
                Console.WriteLine($"[nodejs] {jsCode}");
                return default;
            }
            
            var pack = new NodePack
            {
                Id = s_idGenerator.Next(),
                Type = NodePackType.InvokeCode,
                Content = jsCode
            };

            using var bw = new BinaryWriter(Console.OpenStandardOutput());
            bw.Write(pack.Encode());
            bw.Flush();

            var task = new TaskCompletionSource<T>();
            s_dict[s_idGenerator.Next()] = task;
            
            return task.Task;
        }

        public static void Loop()
        {
            var inputStream = Console.OpenStandardInput();
            using var br = new BinaryReader(inputStream);

            while (true)
            {
                var length = br.ReadInt32();
                var data = br.ReadBytes(length);
                var pack = NodePack.Decode(data);
                ResolvePack(pack);
            }
        }

        public static void ResolvePack(NodePack pack)
        {
            if (pack.Type != NodePackType.InvokeResult) return;
            if (!s_dict.TryRemove(pack.Id, out var obj)) return;
            if (obj.GetType().GenericTypeArguments.Length <= 0) return;

            var resultType = obj.GetType().GenericTypeArguments[0];
            var setResultMethod = typeof(TaskCompletionSource<>).MakeGenericType(resultType)
                .GetMethod(nameof(TaskCompletionSource.SetResult));

            var jsonConvertMethod = typeof(JsonConvert).GetGenericMethod(nameof(JsonConvert.DeserializeObject), new[] {resultType}, typeof(string));
            var result = jsonConvertMethod.Invoke(null, new object?[] {pack.Content});
            setResultMethod!.Invoke(obj, new[] {result});
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

            using var bw = new BinaryWriter(Console.OpenStandardOutput());
            bw.Write(pack.Encode());
            bw.Flush();
        }
    }
}
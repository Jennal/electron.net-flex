using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Reflection;
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
            
            var len = 2 + Content.Length;
            bw.Write(len);
            bw.Write(Id);
            bw.Write((byte)Type);
            bw.Write(Content);
            bw.Flush();

            return ms.ToArray();
        }

        public static NodePack Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            return new NodePack
            {
                Id = br.ReadByte(),
                Type = (NodePackType)br.ReadByte(),
                Content = br.ReadString()
            };
        }
    }
    
    public static class NodeJs
    {
        private static IdGenerator s_idGenerator = new IdGenerator();
        public static ConcurrentDictionary<byte, object> s_dict = new ConcurrentDictionary<byte, object>();
        
        public static void Invoke(string jsCode)
        {
            Invoke<object>(jsCode);
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

            var jsonConvertMethod = typeof(JsonConvert).GetMethod("DeserializeObject")!
                .MakeGenericMethod(resultType);
            var result = jsonConvertMethod.Invoke(null, new object?[] {pack.Content});
            setResultMethod!.Invoke(obj, new[] {result});
        }

        public static void WriteLine(string? line)
        {
            line = line?.TrimEnd('\n');
            line = line?.TrimEnd('\r');
            var pack = new NodePack
            {
                Type = NodePackType.ConsoleOutput,
                Content = line
            };

            using var bw = new BinaryWriter(Console.OpenStandardOutput());
            bw.Write(pack.Encode());
            bw.Flush();
        }
    }
}
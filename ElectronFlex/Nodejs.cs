using System;
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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    class IgnoreReturn
    {}

    public static class NodeJs
    {
        private static IdGenerator s_idGenerator = new IdGenerator();
        private static InvokeTaskManager s_taskManager = new InvokeTaskManager();
        
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

            return s_taskManager.Invoke<T>(pack);
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
                s_taskManager.Result(pack);
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

            using var bw = new BinaryWriter(Console.OpenStandardOutput());
            bw.Write(pack.Encode());
            bw.Flush();
        }
    }
}
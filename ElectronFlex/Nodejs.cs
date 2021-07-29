using System;
using System.IO;
using System.Threading.Tasks;

namespace ElectronFlex
{
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
            if (!Config.CommandLineOptions?.StartFromElectron ?? true)
            {
                Console.WriteLine($"[nodejs] {jsCode}");
                return default;
            }
            
            var pack = new Pack
            {
                Id = s_idGenerator.Next(),
                Type = PackType.InvokeCode,
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
                var pack = Pack.Decode(data);
                s_taskManager.Result(pack);
            }
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

            using var bw = new BinaryWriter(Console.OpenStandardOutput());
            bw.Write(pack.Encode());
            bw.Flush();
        }
    }
}
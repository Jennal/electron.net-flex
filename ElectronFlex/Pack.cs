using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ElectronFlex
{
    public enum PackType : byte
    {
        ConsoleOutput = 0,
        InvokeCode = 1,
        InvokeResult = 2,
    }
    
    public struct Pack
    {
        public byte Id;
        public PackType Type;
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

        public static Pack Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var pack = new Pack
            {
                Id = br.ReadByte(),
                Type = (PackType)br.ReadByte(),
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

    [JsonObject]
    public struct InvokeError
    {
        [JsonProperty("source")]
        public string Source;
        
        [JsonProperty("err")]
        public string Err;

        public static string Error(BrowserInvoke invoke, string error)
        {
            var err = new InvokeError();
            err.Source = JsonConvert.SerializeObject(invoke);
            err.Err = error;

            return JsonConvert.SerializeObject(err);
        }
    }
}
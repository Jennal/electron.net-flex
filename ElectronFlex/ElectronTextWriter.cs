using System;
using System.IO;
using System.Text;

namespace ElectronFlex {
    class ElectronTextWriter : StreamWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public ElectronTextWriter(Stream stream): base(stream, Encoding.UTF8.RemovePreamble(), 256, true)
        {
        }

        public override void WriteLine(string? value)
        {
            if (Config.CommandLineOptions.StartFromElectron)
            {
                NodeJs.WriteLine(value);
            }
            else
            {
                base.WriteLine(value);
            }
        }
    }
    
    internal static class EncodingExtensions
    {
        public static Encoding RemovePreamble(this Encoding encoding)
        {
            if (encoding.Preamble.Length == 0)
            {
                return encoding;
            }
            return new ConsoleEncoding(encoding);
        }
    }

    // StreamWriter calls Encoding.GetPreamble() to write the initial bits to the stream.
    // In case of Console we do not want to write the preamble as the user does not expect these bits.
    // In .NET Framework this is handled by setting an internal property on the StreamWriter HasPreambleBeenWritten = true
    // Since portable library does not have access to the internal property we wrap the encoding into a ConsoleEncoding
    // which delegates every call to the original encoding except GetPreamble() which does not do anything.
    internal sealed class ConsoleEncoding : Encoding
    {
        private readonly Encoding _encoding;

        internal ConsoleEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }

        public override byte[] GetPreamble()
        {
            return Array.Empty<byte>();
        }

        public override int CodePage => _encoding.CodePage;

        public override bool IsSingleByte => _encoding.IsSingleByte;

        public override string EncodingName => _encoding.EncodingName;

        public override string WebName => _encoding.WebName;

        public override int GetByteCount(char[] chars)
        {
            return _encoding.GetByteCount(chars);
        }

        public override unsafe int GetByteCount(char* chars, int count)
        {
            return _encoding.GetByteCount(chars, count);
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return _encoding.GetByteCount(chars, index, count);
        }

        public override int GetByteCount(string s)
        {
            return _encoding.GetByteCount(s);
        }

        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            return _encoding.GetBytes(chars, charCount, bytes, byteCount);
        }

        public override byte[] GetBytes(char[] chars)
        {
            return _encoding.GetBytes(chars);
        }

        public override byte[] GetBytes(char[] chars, int index, int count)
        {
            return _encoding.GetBytes(chars, index, count);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override byte[] GetBytes(string s)
        {
            return _encoding.GetBytes(s);
        }

        public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return _encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
        }

        public override unsafe int GetCharCount(byte* bytes, int count)
        {
            return _encoding.GetCharCount(bytes, count);
        }

        public override int GetCharCount(byte[] bytes)
        {
            return _encoding.GetCharCount(bytes);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return _encoding.GetCharCount(bytes, index, count);
        }

        public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            return _encoding.GetChars(bytes, byteCount, chars, charCount);
        }

        public override char[] GetChars(byte[] bytes)
        {
            return _encoding.GetChars(bytes);
        }

        public override char[] GetChars(byte[] bytes, int index, int count)
        {
            return _encoding.GetChars(bytes, index, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override Decoder GetDecoder()
        {
            return _encoding.GetDecoder();
        }

        public override Encoder GetEncoder()
        {
            return _encoding.GetEncoder();
        }

        public override int GetMaxByteCount(int charCount)
        {
            return _encoding.GetMaxByteCount(charCount);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return _encoding.GetMaxCharCount(byteCount);
        }

        public override string GetString(byte[] bytes)
        {
            return _encoding.GetString(bytes);
        }

        public override string GetString(byte[] bytes, int index, int count)
        {
            return _encoding.GetString(bytes, index, count);
        }
    }
}
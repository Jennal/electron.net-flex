using System;
using System.IO;

namespace ElectronFlex
{
    public class WebSocketStream
    {
        public const int INIT_SIZE = 1024 * 128; //128k
        public const int CHUNK_SIZE = 1024 * 16; //16k

        private byte[] _buffer = new byte[INIT_SIZE];
        private MemoryStream _readStream;
        private MemoryStream _writeStream;

        private BinaryReader _reader;
        private BinaryWriter _writer;

        public WebSocketStream()
        {
            SetBuffer(_buffer);
        }

        private void SetBuffer(byte[] buffer)
        {
            _buffer = buffer;
        
            _reader?.Close();
            _writer?.Close();

            _readStream?.Close();
            _writeStream?.Close();

            _readStream = new MemoryStream(buffer);
            _writeStream = new MemoryStream(buffer);

            _reader = new BinaryReader(_readStream);
            _writer = new BinaryWriter(_writeStream);
        }

        private void CheckSize(int size)
        {
            var rpos = _readStream.Seek(0, SeekOrigin.Current);
            var wpos = _writeStream.Seek(0, SeekOrigin.Current);
            if (wpos + size < _buffer.Length) return;

            var newSize = Math.Max(INIT_SIZE, wpos + size - rpos);
            while (wpos + size - rpos >= newSize) newSize += CHUNK_SIZE;

            var newBuff = new byte[newSize];
            Array.Copy(_buffer, rpos, newBuff, 0, wpos-rpos);
            
            SetBuffer(newBuff);
        }

        public bool HasSizeForRead(int size)
        {
            var rpos = _readStream.Seek(0, SeekOrigin.Current);
            var wpos = _writeStream.Seek(0, SeekOrigin.Current);
            return wpos - rpos >= size;
        }

        public void UnReadInt32()
        {
            _readStream.Seek(-1 * sizeof(int), SeekOrigin.Current);
        }
        
        public int ReadInt32() => _reader.ReadInt32();
        public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

        public void WriteInt32(int val)
        {
            CheckSize(sizeof(int));
            _writer.Write(val);
        }

        public void WriteBytes(byte[] val)
        {
            CheckSize(val.Length);
            _writer.Write(val);
        }
    }
}
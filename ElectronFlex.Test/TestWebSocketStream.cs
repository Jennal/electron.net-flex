using NUnit.Framework;

namespace ElectronFlex.Test
{
    public class TestWebSocketStream
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            var s = new WebSocketStream();
            s.SetBuffer(new byte[4]);
            Assert.AreEqual(4, s.BufferSize);
            Assert.AreEqual(0, s.ContentSize);
            Assert.AreEqual(false, s.HasSizeForRead(sizeof(int)));
            
            s.WriteInt32(1);
            Assert.AreEqual(4, s.BufferSize);
            Assert.AreEqual(4, s.ContentSize);
            Assert.AreEqual(true, s.HasSizeForRead(sizeof(int)));
            
            var data = s.ReadBytes(4);
            Assert.AreEqual(4, s.BufferSize);
            Assert.AreEqual(0, s.ContentSize);
            Assert.AreEqual(new byte[] {1, 0, 0, 0}, data);
            
            s.UnReadInt32();
            Assert.AreEqual(4, s.BufferSize);
            Assert.AreEqual(4, s.ContentSize);

            var val = s.ReadInt32();
            Assert.AreEqual(4, s.BufferSize);
            Assert.AreEqual(0, s.ContentSize);
            Assert.AreEqual(1, val);
        }
    }
}
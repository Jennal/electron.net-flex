using NUnit.Framework;

namespace ElectronFlex.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Config.CommandLineOptions = new CommandLineOptions
            {
                StartFromElectron = true
            };
        }

        [Test]
        public void TestCallback()
        {
            var task = NodeJs.Invoke<string>("hello");
            Assert.AreEqual(1, NodeJs.s_dict.Count);
            NodeJs.ResolvePack(new NodePack
            {
                Type = NodePackType.InvokeResult,
                Content = "\"hello\""
            });
            task.Wait();
            Assert.AreEqual("hello", task.Result);
        }
    }
}
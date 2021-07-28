using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ElectronFlex.Test
{
    public class TestBrowserJs
    {
        public static Task<int> Get()
        {
            var task = new TaskCompletionSource<int>();
            task.SetResult(100);
            return task.Task;
        }
        
        public static Task<int> Get<T>()
        {
            var task = new TaskCompletionSource<int>();
            task.SetResult(101);
            return task.Task;
        }
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestInvoke()
        {
            var result = BrowserJs.DoInvoke(new Pack
            {
                Content = JsonConvert.SerializeObject(new BrowserInvoke
                {
                    Class = "ElectronFlex.Test.TestBrowserJs",
                    Method = "Get",
                    Arguments = new object[] { }
                })
            });
            Assert.AreEqual(100, result);
            
            result = BrowserJs.DoInvoke(new Pack
            {
                Content = JsonConvert.SerializeObject(new BrowserInvoke
                {
                    Class = "ElectronFlex.Test.TestBrowserJs",
                    Method = "Get<int>",
                    Arguments = new object[] { }
                })
            });
            Assert.AreEqual(101, result);
        }
    }
}
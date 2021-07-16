using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ElectronFlex
{
    public class InvokeTaskManager
    {
        public ConcurrentDictionary<byte, object> _dict = new ConcurrentDictionary<byte, object>();

        public Task<T> Invoke<T>(NodePack pack)
        {
            var task = new TaskCompletionSource<T>();
            _dict[pack.Id] = task;
            return task.Task;
        }

        public void Result(NodePack pack)
        {
            if (pack.Type != NodePackType.InvokeResult) return;
            if (!_dict.TryRemove(pack.Id, out var obj)) return;
            if (obj.GetType().GenericTypeArguments.Length <= 0) return;

            var resultType = obj.GetType().GenericTypeArguments[0];
            var setResultMethod = typeof(TaskCompletionSource<>).MakeGenericType(resultType)
                .GetMethod(nameof(TaskCompletionSource.SetResult));

            var jsonConvertMethod = typeof(JsonConvert).GetGenericMethod(nameof(JsonConvert.DeserializeObject), new[] {resultType}, typeof(string));
            var result = jsonConvertMethod.Invoke(null, new object?[] {pack.Content});
            setResultMethod!.Invoke(obj, new[] {result});
        }
    }
}
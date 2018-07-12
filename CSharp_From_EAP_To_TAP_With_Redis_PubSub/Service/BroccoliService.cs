using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CSharp_From_EAP_To_TAP_With_Redis_PubSub
{
    public class BroccoliService
    {
        private IConnectionMultiplexer _connection;
        private ISubscriber _subscriber;
        private ConcurrentDictionary<Guid, Action<PubSubResponseMessage>> _messageCallbacks = new ConcurrentDictionary<Guid, Action<PubSubResponseMessage>>();
        private string _requestChannel;
        private string _responseChannel;

        public BroccoliService(IConnectionMultiplexer connection, string requestChannel, string responseChannel)
        {
            _connection = connection;
            _requestChannel = requestChannel;
            _responseChannel = responseChannel;
            _subscriber = _connection.GetSubscriber();
        }

        public void ListenResponses()
        {
            _subscriber.Subscribe(_responseChannel, ResponseCallback);
        }

        private void ResponseCallback(RedisChannel chl, RedisValue msg)
        {
            PubSubResponseMessage response = JsonConvert.DeserializeObject<PubSubResponseMessage>(msg);

            _messageCallbacks.TryGetValue(response.CorrelationId, out Action<PubSubResponseMessage> waitingCallback);

            waitingCallback?.Invoke(response);
        }

        public async Task<string> GetMenu(int menuId)
        {
            PubSubRequestMessage req = new PubSubRequestMessage() { Value = menuId, ResponseChannel = _responseChannel };

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            _messageCallbacks.TryAdd(req.CorrelationId, response => tcs.SetResult(response.ResponseValue));

            long numberOfClients = await _subscriber.PublishAsync(_requestChannel, JsonConvert.SerializeObject(req));

            return await tcs.Task;
        }
    }
}
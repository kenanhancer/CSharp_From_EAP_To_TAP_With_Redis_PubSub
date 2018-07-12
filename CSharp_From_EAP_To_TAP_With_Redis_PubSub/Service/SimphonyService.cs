using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading;

namespace CSharp_From_EAP_To_TAP_With_Redis_PubSub
{
    public class SimphonyService
    {
        private IConnectionMultiplexer _connection;
        private ISubscriber _subscriber;
        private string _requestChannel;

        public SimphonyService(IConnectionMultiplexer connection, string requestChannel)
        {
            _connection = connection;
            _requestChannel = requestChannel;
            _subscriber = connection.GetSubscriber();
        }

        public void ListenRequests()
        {
            _subscriber.Subscribe(_requestChannel, RequestCallback);
        }

        private void RequestCallback(RedisChannel chl, RedisValue msg)
        {
            PubSubRequestMessage requestMessage = JsonConvert.DeserializeObject<PubSubRequestMessage>(msg);

            Thread.Sleep(100 + new Random().Next(500, 2000));

            PubSubResponseMessage responseMessage = new PubSubResponseMessage()
            {
                CorrelationId = requestMessage.CorrelationId,
                ResponseValue = $"Hello {requestMessage.Value}"
            };

            string message = JsonConvert.SerializeObject(responseMessage);

            _subscriber.Publish(requestMessage.ResponseChannel, message);
        }
    }
}
using System;

namespace CSharp_From_EAP_To_TAP_With_Redis_PubSub
{
    public abstract class PubSubCorrelatedMessage
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public string ResponseChannel { get; set; }
    }
}
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility
{
    public class CustomMessageBus : IMessageBus
    {
        private readonly IMessageBus _innerBus;
        private readonly List<IMessageSinkMessage> messages = new List<IMessageSinkMessage>();

        public CustomMessageBus(IMessageBus innerBus)
        {
            this._innerBus = innerBus;
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            lock (messages)
            {
                _innerBus.QueueMessage(message);
            }

            return true;
        }

        public void Dispose()
        {
        }
    }
}

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{

    internal class DatabaseAdditionalConnectionStateMessage : ValueChangedMessage<string>
    {
        public DatabaseAdditionalConnectionStateMessage(string value) : base(value) { }
    }
}


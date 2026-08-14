using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{

    internal class DatabaseConnectionStateMessage : ValueChangedMessage<bool>
    {
        public DatabaseConnectionStateMessage(bool value) : base(value) { }
    }
}

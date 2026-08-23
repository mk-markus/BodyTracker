using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{
    internal class ActiveDatabaseNameMessage : ValueChangedMessage<string>
    {
        public ActiveDatabaseNameMessage(string value) : base(value) { }
    }
}
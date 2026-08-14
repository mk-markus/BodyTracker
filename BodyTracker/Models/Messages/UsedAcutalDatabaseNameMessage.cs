using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{
    internal class UsedAcutalDatabaseNameMessage : ValueChangedMessage<string>
    {
        public UsedAcutalDatabaseNameMessage(string value) : base(value) { }
    }
}
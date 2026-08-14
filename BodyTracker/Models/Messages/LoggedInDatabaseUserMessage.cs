using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{
    internal class LoggedInDatabaseUserMessage : ValueChangedMessage<string>
    {
        public LoggedInDatabaseUserMessage(string value) : base(value) { }
    }
}
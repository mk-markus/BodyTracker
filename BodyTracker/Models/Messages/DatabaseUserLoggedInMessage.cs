using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{
    internal class DatabaseUserLoggedInMessage : ValueChangedMessage<string>
    {
        public DatabaseUserLoggedInMessage(string value) : base(value) { }
    }
}
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{
    internal class DatabaseErrorMessage: ValueChangedMessage<string>
    {
        public DatabaseErrorMessage(string value) : base(value) { }
    }
}

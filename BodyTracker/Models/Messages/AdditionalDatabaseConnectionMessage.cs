using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BodyTracker.Services
{

    internal class AdditionalDatabaseConnectionMessage : ValueChangedMessage<string>
    {
        public AdditionalDatabaseConnectionMessage(string value) : base(value) { }
    }
}


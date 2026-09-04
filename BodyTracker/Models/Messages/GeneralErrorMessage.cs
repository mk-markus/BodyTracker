using CommunityToolkit.Mvvm.Messaging.Messages;

public sealed class GeneralInfoMessage : ValueChangedMessage<string>
{
    public GeneralInfoMessage(string value) : base(value) { }
}

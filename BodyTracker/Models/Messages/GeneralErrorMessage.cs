using CommunityToolkit.Mvvm.Messaging.Messages;

public sealed class GeneralErrorMessage : ValueChangedMessage<string>
{
    public GeneralErrorMessage(string value) : base(value) { }
}

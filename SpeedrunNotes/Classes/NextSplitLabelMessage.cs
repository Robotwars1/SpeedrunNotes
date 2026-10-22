using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class NextSplitLabelMessage : ValueChangedMessage<string>
    {
        public NextSplitLabelMessage(string Text) : base(Text) { }
    }
}

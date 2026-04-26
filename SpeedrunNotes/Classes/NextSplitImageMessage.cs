using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class NextSplitImageMessage : ValueChangedMessage<string>
    {
        public NextSplitImageMessage(string Path) : base(Path) { }
    }
}

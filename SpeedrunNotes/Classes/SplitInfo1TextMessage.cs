using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class SplitInfo1TextMessage : ValueChangedMessage<string>
    {
        public SplitInfo1TextMessage(string Path) : base(Path) { }
    }
}

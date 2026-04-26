using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class SplitInfo2TextMessage : ValueChangedMessage<string>
    {
        public SplitInfo2TextMessage(string Path) : base(Path) { }
    }
}

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class SplitInfo2FontMessage : ValueChangedMessage<int>
    {
        public SplitInfo2FontMessage(int Path) : base(Path) { }
    }
}

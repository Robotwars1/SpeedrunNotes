using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SpeedrunNotes
{
    internal class SplitInfo1FontMessage : ValueChangedMessage<int>
    {
        public SplitInfo1FontMessage(int Path) : base(Path) { }
    }
}

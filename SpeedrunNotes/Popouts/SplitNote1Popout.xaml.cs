using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes.Popouts;

public partial class SplitNote1Popout : BasePopout
{
    int FontSize;
    string SplitNoteLabelText;

    public SplitNote1Popout()
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<SplitInfo1FontMessage>(this, (r, m) =>
        {
            FontSize = m.Value;
        });

        WeakReferenceMessenger.Default.Register<SplitInfo1TextMessage>(this, (r, m) =>
        {
            SplitNoteLabelText = m.Value;
        });

        InitTimer();
    }

    public override void UpdateData()
    {
        SplitNoteLabel.Text = SplitNoteLabelText;
        SplitNoteLabel.FontSize = FontSize;
    }
}

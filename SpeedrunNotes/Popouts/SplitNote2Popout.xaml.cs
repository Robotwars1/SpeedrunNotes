using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes.Popouts;

public partial class SplitNote2Popout : BasePopout
{
    int FontSize;
    string SplitNoteLabelText;

    public SplitNote2Popout()
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<SplitInfo2FontMessage>(this, (r, m) =>
        {
            FontSize = m.Value;
        });

        WeakReferenceMessenger.Default.Register<SplitInfo2TextMessage>(this, (r, m) =>
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

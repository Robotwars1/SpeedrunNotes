using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes.Popouts;

public partial class NextSplitPopout : BasePopout
{
    string NextSplitLabelText;
    string NextSplitImageFilePath;

    string PreviousTitle;

    public NextSplitPopout()
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<NextSplitLabelMessage>(this, (r, m) =>
        {
            NextSplitLabelText = m.Value;
        });

        WeakReferenceMessenger.Default.Register<NextSplitImageMessage>(this, (r, m) =>
        {
            NextSplitImageFilePath = m.Value;
        });

        InitTimer();
    }

    public override void UpdateData()
    {
        if (File.Exists(NextSplitImageFilePath))
        {
            // Only redraw if something changed
            if (PreviousTitle != NextSplitLabelText)
            {
                // Update title and image of next split
                NextSplitLabel.Text = NextSplitLabelText;

                NextSplitImage.Source = ImageSource.FromFile(NextSplitImageFilePath);

                PreviousTitle = NextSplitLabel.Text;
            }
        }
        else
        {
            // Only redraw if something changed
            if (PreviousTitle != NextSplitLabelText)
            {
                // Update title and image of next split
                NextSplitLabel.Text = NextSplitLabelText;
                NextSplitImage.Source = "imageloadfail.png";

                PreviousTitle = NextSplitLabel.Text;
            }
        }
    }
}

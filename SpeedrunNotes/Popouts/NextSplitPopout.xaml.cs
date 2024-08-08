using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes.Popouts;

public partial class NextSplitPopout : ContentPage
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 320;
        Window.MinimumHeight = 200;
    }

    public void InitTimer()
    {
        // Setup timer to send / recieve message with LiveSplit.Server every second
        System.Timers.Timer Timer = new(1000);
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    void OnTimedEvent(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateNextSplit);
    }

    void UpdateNextSplit()
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

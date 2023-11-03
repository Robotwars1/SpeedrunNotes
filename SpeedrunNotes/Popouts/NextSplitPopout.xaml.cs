namespace SpeedrunNotes.Popouts;

public partial class NextSplitPopout : ContentPage
{
    string NextSplitLabelText;
    string NextSplitImageFilePath;

    string PreviousTitle;

    public NextSplitPopout()
	{
		InitializeComponent();

        MessagingCenter.Subscribe<MainPage, string>(this, "NextSplitLabel", (sender, arg) =>
        {
            // Do something whenever the message is received
            NextSplitLabelText = arg;
        });

        MessagingCenter.Subscribe<MainPage, string>(this, "NextSplitImage", (sender, arg) =>
        {
            // Do something whenever the message is received
            NextSplitImageFilePath = arg;
        });

        InitTimer();
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

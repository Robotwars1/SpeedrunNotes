namespace SpeedrunNotes.Popouts;

public partial class SplitNote1Popout : ContentPage
{
    int FontSize;
    string SplitNoteLabelText;

    public SplitNote1Popout()
	{
		InitializeComponent();

        MessagingCenter.Subscribe<MainPage, int>(this, "SplitNote1FontSize", (sender, arg) =>
        {
            // Do something whenever the message is received
            FontSize = arg;
        });

        MessagingCenter.Subscribe<MainPage, string>(this, "SplitNote1Label", (sender, arg) =>
        {
            // Do something whenever the message is received
            SplitNoteLabelText = arg;
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
        SplitNoteLabel.Text = SplitNoteLabelText;
        SplitNoteLabel.FontSize = FontSize;
    }
}

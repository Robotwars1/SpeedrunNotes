namespace SpeedrunNotes.Popouts;

public partial class SplitNote2Popout : ContentPage
{
    int FontSize;
    string SplitNoteLabelText;

    public SplitNote2Popout()
	{
		InitializeComponent();

        MessagingCenter.Subscribe<MainPage, int>(this, "SplitNote2FontSize", (sender, arg) =>
        {
            // Do something whenever the message is received
            FontSize = arg;
        });

        MessagingCenter.Subscribe<MainPage, string>(this, "SplitNote2Label", (sender, arg) =>
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

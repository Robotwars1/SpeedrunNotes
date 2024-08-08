using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes.Popouts;

public partial class SplitNote1Popout : ContentPage
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
        SplitNoteLabel.Text = SplitNoteLabelText;
        SplitNoteLabel.FontSize = FontSize;
    }
}

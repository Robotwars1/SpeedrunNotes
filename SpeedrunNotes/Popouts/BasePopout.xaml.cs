namespace SpeedrunNotes.Popouts;

public abstract partial class BasePopout : ContentPage
{
	public BasePopout()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 320;
        Window.MinimumHeight = 200;
        Window.MaximumWidth = 1080;
        Window.MaximumHeight = 720;
    }

    public void InitTimer()
    {
        // Setup timer to send / recieve message with LiveSplit.Server every second
        System.Timers.Timer Timer = new(1000);
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private void OnTimedEvent(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateData);
    }

    public abstract void UpdateData();
}

namespace SpeedrunNotes;

public partial class ConnectionPage : ContentPage
{
	bool ConnectionError;

	public ConnectionPage(bool connectionError)
	{
		ConnectionError = connectionError;

		InitializeComponent();

		// Set IP and Port to previously used value
		IPEntry.Text = Preferences.Default.Get("IP", "localhost");
        PortEntry.Text = Preferences.Default.Get("Port", 16834).ToString();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 720;
    }

    void OnConnectionPageAppearing(object sender, EventArgs e)
	{
		if (ConnectionError == true)
		{
			// Show ConnectionError
			ConnectionErrorBackground.IsVisible = true;
			ConnectionErrorLabel.IsVisible = true;
		}
		else
		{
            // Hide ConnectionError
            ConnectionErrorBackground.IsVisible = false;
            ConnectionErrorLabel.IsVisible = false;
        }

        InfoStuffBorder.IsVisible = false;
		ConnectionErrorTroubleshootingBorder.IsVisible = false;

        ConnectButton.Text = "Connect";
    }

	void OnIpChanged(object sender, EventArgs e)
	{
		Preferences.Default.Set("IP", IPEntry.Text);
	}

	void OnIpResetButtonClicked(object sender, EventArgs e)
	{
		IPEntry.Text = "localhost";
		Preferences.Default.Set("IP", "localhost");
	}

    void OnPortChanged(object sender, EventArgs e)
	{
		Preferences.Default.Set("Port", int.Parse(PortEntry.Text));
	}

	void OnPortResetButtonClicked(object sender, EventArgs e)
	{
        PortEntry.Text = "16834";
        Preferences.Default.Set("Port", 16834);
    }

    async void OnConnectButtonClicked(object sender, EventArgs e)
	{
		ConnectButton.Text = "Connecting...";

		// Waits 5 ms to make sure ConnectButton.Text is updated
		// Yes, this is technicly bad and slow but offering 5 ms to make visual work is a-ok
		await Task.Delay(5);

		await Navigation.PopModalAsync();
    }

	void OnInfoToggleButtonClicked(object sender, EventArgs e)
	{
		// Toggle visibility by setting it to opposite of current value
		InfoStuffBorder.IsVisible = !InfoStuffBorder.IsVisible;
    }

	void OnConnectionErrorTroubleshootingButtonClicked(object sender, EventArgs e)
	{
        // Toggle visibility by setting it to opposite of current value
        ConnectionErrorTroubleshootingBorder.IsVisible = !ConnectionErrorTroubleshootingBorder.IsVisible;
    }
}

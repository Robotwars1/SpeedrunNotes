namespace SpeedrunNotes;

public partial class ConnectionPage : ContentPage
{
	public ConnectionPage()
	{
		InitializeComponent();

		// Set IP and Port to previously used value
		IPEntry.Text = Preferences.Default.Get("IP", "localhost");
        PortEntry.Text = Preferences.Default.Get("Port", 16834).ToString();
    }

	void OnConnectionPageAppearing(object sender, EventArgs e)
	{
		if (Preferences.Default.Get("ConnectionError", false) == true)
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

    // Go back to MainPage
    async void OnConnectButtonClicked(object sender, EventArgs e)
	{
		ConnectButton.Text = "Connecting...";

		// Waits 5 ms to make sure ConnectButton.Text is updated
		// Yes, this is technicly bad and slow but offering 5 ms to make visual work is a-ok
		await Task.Delay(5);

		await Navigation.PopModalAsync();
    }
}

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
	}

	void OnIpChanged(object sender, EventArgs e)
	{
		Preferences.Default.Set("IP", IPEntry.Text);
	}

	void OnPortChanged(object sender, EventArgs e)
	{
		Preferences.Default.Set("Port", int.Parse(PortEntry.Text));
	}

    // Go back to MainPage
    void OnConnectButtonClicked(object sender, EventArgs e)
	{
		Navigation.PopModalAsync();
    }
}

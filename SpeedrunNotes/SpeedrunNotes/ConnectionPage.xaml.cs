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

	// Go back to MainPage
	void OnConnectButtonClicked(object sender, EventArgs e)
	{
		Navigation.PopModalAsync();
	}
}

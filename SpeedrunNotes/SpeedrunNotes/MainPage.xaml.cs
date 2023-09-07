using System.Net.Sockets;
using System.Net;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
	bool FirstAppear = true;

	public MainPage()
	{
		InitializeComponent();
	}

	// When loaded, open the ConnectionPage
	void OnMainPageLoaded(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new ConnectionPage());
	}

	void OnMainPageAppearing(object sender, EventArgs e)
	{
		// If not first time it appears, eg when going from ConnectionPage to MainPage
        if (!FirstAppear)
        {
			try
			{
                // Setup Socket, IP and Port
                Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip;

                // If IP is localhost, input the IP for localhost, eg 127.0.0.1
                if (Preferences.Default.Get("IP", "localhost") == "localhost")
                {
                    ip = IPAddress.Parse("127.0.0.1");
                }
                else
                {
                    ip = IPAddress.Parse(Preferences.Default.Get("IP", "localhost"));
                }

                IPEndPoint remoteEP = new IPEndPoint(ip, (Preferences.Default.Get("Port", 16834)));

                // Connect to livesplit.server
                soc.Connect(remoteEP);
            }
			catch
			{
				// Show ConnectionError, bring back to ConnectionPage
			}
        }

		FirstAppear = false;
	}
}

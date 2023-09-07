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
			// Setup Socket, IP and Port
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress ip = IPAddress.Parse(Preferences.Default.Get("IP", "localhost"));
            IPEndPoint remoteEP = new IPEndPoint(ip, (Preferences.Default.Get("Port", 16834)));
            
			// Connect to livesplit.server
			soc.Connect(remoteEP);
        }

		FirstAppear = false;
	}
}

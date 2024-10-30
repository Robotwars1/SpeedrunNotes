using System.Net.Sockets;
using System.Net;
using System.Text;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 720;
    }

    void OnConnectionPageAppearing(object sender, EventArgs e)
	{
        //ConnectButton.Text = "Connect";
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

		SetupConnection();
    }

    void SetupConnection()
    {
        Socket soc;

        try
        {
            // Setup Socket, IP and Port
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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

            IPEndPoint remoteEP = new(ip, (Preferences.Default.Get("Port", 16834)));

            // Connect to livesplit.server
            soc.Connect(remoteEP);
        }
        catch
        {
            // Connection error!
            ConnectionErrorBackground.IsVisible = true;
            ConnectButton.Text = "Connect";
            return;
        }

        CheckConnection(soc);
    }

    void CheckConnection(Socket soc)
    {
        try
        {
            // Send message to livesplit.server to check current split
            byte[] message = Encoding.ASCII.GetBytes("getsplitindex\r\n");
            soc.Send(message);

            // Recieve message and "parse" it from computer-jargon -> readable string
            byte[] b = new byte[100];
            int k = soc.Receive(b);
            string DataReceived = Encoding.ASCII.GetString(b, 0, k);

            int ReceivedMessage = 0;

            // Makes sure the whole message is recieved
            if (DataReceived.EndsWith("\r\n"))
            {
                // Only remove the last 2 instead of last 4 for some reason that I do not understand, removes the "\r\n" tho so thats good
                // Thanks alekz for this :)
                string Temp = DataReceived.Remove(DataReceived.Length - 2, 2);

                // Save recieved split-index
                ReceivedMessage = int.Parse(Temp);
            }

            // If the wrong message is received
            if (ReceivedMessage != -1)
            {
                throw new Exception("ConnectionError");
            }
        }
        catch
        {
            // Connection error!
            ConnectionErrorBackground.IsVisible = true;
            ConnectButton.Text = "Connect";
            return;
        }

        Navigation.PopModalAsync();
    }
}

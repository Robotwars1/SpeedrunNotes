using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
	bool FirstAppear = true;

    Socket soc;

    public class Split
    {
        public string SplitTitle { get; set; }
        public string SplitImage { get; set; }
        public string SplitInfoText { get; set; }
        public string SplitInfoImage { get; set; }
    }

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
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IPEndPoint remoteEP = new IPEndPoint(ip, (Preferences.Default.Get("Port", 16834)));
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 16834);

            // Connect to livesplit.server
            soc.Connect(remoteEP);

            // Start the timer / scheduled function calls
            InitTimer();

            /*
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

                //IPEndPoint remoteEP = new IPEndPoint(ip, (Preferences.Default.Get("Port", 16834)));
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 16834);

                // Connect to livesplit.server
                soc.Connect(remoteEP);

                // Start the timer / scheduled function calls
                InitTimer();
            }
			catch
			{
				// Show ConnectionError, bring back to ConnectionPage
			}
            */
        }

		FirstAppear = false;
	}

    public void InitTimer()
    {
        System.Timers.Timer timer1 = new System.Timers.Timer(1000);
        timer1.Elapsed += OnTimedEvent;
        timer1.AutoReset = true;
        timer1.Enabled = true;
    }

    // Called once per second
    private void OnTimedEvent(object sender, EventArgs e)
    {
        // Send message to livesplit.server to check current split
        byte[] message = System.Text.Encoding.ASCII.GetBytes("getsplitindex\r\n");
        soc.Send(message);

        byte[] b = new byte[100];
        int k = soc.Receive(b);
        string szReceived = Encoding.ASCII.GetString(b, 0, k);
    }

    public List<Split> JSONParse(string FilePath)
    {
        using FileStream json = File.OpenRead(FilePath);
        List<Split> Splits = JsonSerializer.Deserialize<List<Split>>(json);
        return Splits;
    }

    void OnReconnectBtnClicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new ConnectionPage());
	}

    async void OnLoadPresetBtnClicked(object sender, EventArgs e)
    {
        var File = await FilePicker.PickAsync(default);
        string FilePath = File.FullPath;
        List<Split> SplitsInfo = JSONParse(FilePath);
    }
}

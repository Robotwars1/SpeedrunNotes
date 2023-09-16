﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
	bool FirstAppear = true;

    int CurrentSplitIndex;

    Socket soc;

    public class Split
    {
        public string SplitTitle { get; set; }
        public string SplitImage { get; set; }
        public string SplitInfoText { get; set; }
        public string SplitInfoImage { get; set; }
    }

    List<Split> SplitsInfo;

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

        // Recieve message and "parse" it from computer-jargon -> readable string
        byte[] b = new byte[100];
        int k = soc.Receive(b);
        string DataReceived = Encoding.ASCII.GetString(b, 0, k);

        // Save recieved split-index
        // Makes sure the whole message is recieved
        if (DataReceived.EndsWith("\r\n"))
        {
            // Only remove the last 2 instead of last 4 for some reason that I do not understand, removes the "\r\n" tho so thats good
            // Thanks alekz for this :)
            string Temp = DataReceived.Remove(DataReceived.Length - 2, 2);

            CurrentSplitIndex = int.Parse(Temp);
        }

        // Do UI update stuff, has to be on main thread cause Maui ig
        MainThread.BeginInvokeOnMainThread(UpdateUiElements);
    }

    void UpdateUiElements()
    {
        try
        {
            // Update title and image of next split
            NextSplitLabel.Text = SplitsInfo[CurrentSplitIndex + 1].SplitTitle;
            NextSplitImage.Source = SplitsInfo[CurrentSplitIndex + 1].SplitImage;
        }
        catch
        {

        }
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
        SplitsInfo = JSONParse(FilePath);
    }
}

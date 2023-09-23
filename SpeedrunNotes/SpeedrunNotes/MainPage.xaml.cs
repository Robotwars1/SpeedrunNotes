using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Reflection;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
	bool FirstAppear = true;

    bool TemplateLoaded = false;

    int CurrentSplitIndex;

    string PreviousTitle;

    Socket soc;

    List<Split> SplitsInfo;

    // ONLY FOR RUNING WITH DEBUGGER, NOT VERIFIED FOR BUILD VERSION
    string ImagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\..\..\Images");

    public class Split
    {
        public string SplitTitle { get; set; } = string.Empty;
        public string SplitImage { get; set; } = string.Empty;
        public string SplitInfoText1 { get; set; } = string.Empty;
        public string SplitInfoText2 { get; set; } = string.Empty;
        public string SplitInfoImage1 { get; set; } = string.Empty;
        public string SplitInfoImage2 { get; set; } = string.Empty;
    }

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

                // Start the timer / scheduled function calls
                InitTimer();
            }
			catch
			{
                // Update variable to show "ConnectionError" on ConnectionPage
                Preferences.Default.Set("ConnectionError", true);

                // Bring back to ConnectionPage
                Navigation.PushModalAsync(new ConnectionPage());
            }
        }

		FirstAppear = false;
	}

    // Setup timer to send / recieve message with LiveSplit.Server every second
    public void InitTimer()
    {
        System.Timers.Timer timer1 = new(1000);
        timer1.Elapsed += OnTimedEvent;
        timer1.AutoReset = true;
        timer1.Enabled = true;
    }

    private void OnTimedEvent(object sender, EventArgs e)
    {
        // So it only does update stuff if there is a TemplateLoaded
        if (TemplateLoaded)
        {
            // Send message to livesplit.server to check current split
            byte[] message = Encoding.ASCII.GetBytes("getsplitindex\r\n");
            soc.Send(message);

            // Recieve message and "parse" it from computer-jargon -> readable string
            byte[] b = new byte[100];
            int k = soc.Receive(b);
            string DataReceived = Encoding.ASCII.GetString(b, 0, k);

            // Makes sure the whole message is recieved
            if (DataReceived.EndsWith("\r\n"))
            {
                // Only remove the last 2 instead of last 4 for some reason that I do not understand, removes the "\r\n" tho so thats good
                // Thanks alekz for this :)
                string Temp = DataReceived.Remove(DataReceived.Length - 2, 2);

                // Save recieved split-index
                CurrentSplitIndex = int.Parse(Temp);
            }

            // Do UI update stuff, has to be on main thread cause Maui ig
            MainThread.BeginInvokeOnMainThread(UpdateUiElements);
        }
    }

    void UpdateUiElements()
    {
        try
        {
            if (File.Exists(Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex + 1].SplitImage)))
            {
                // Only redraw if something changed
                if (PreviousTitle != $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].SplitTitle}")
                {
                    // Update title and image of next split
                    NextSplitLabel.Text = $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].SplitTitle}";
                    NextSplitImage.Source = ImageSource.FromFile(SplitsInfo[CurrentSplitIndex + 1].SplitImage);

                    PreviousTitle = NextSplitLabel.Text;
                }
            }
            else
            {
                // Only redraw if something changed
                if (PreviousTitle != $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].SplitTitle}")
                {
                    // Update title and image of next split
                    NextSplitLabel.Text = $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].SplitTitle}";
                    NextSplitImage.Source = "imageloadfail.png";

                    PreviousTitle = NextSplitLabel.Text;
                }
            }
        }
        catch
        {

        }

        try
        {
            // Only do stuff if its within range of list
            if (CurrentSplitIndex > 0)
            {
                // Update notes for current split
                SplitNoteLabel1.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText1;
                SplitNoteLabel2.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText2;
                SplitNoteImage1.Source = ImageSource.FromFile(SplitsInfo[CurrentSplitIndex].SplitInfoImage1);
                SplitNoteImage2.Source = ImageSource.FromFile(SplitsInfo[CurrentSplitIndex].SplitInfoImage2);
            }
        }
        catch
        {

        }
    }

    public List<Split> JSONParse(string FilePath)
    {
        using FileStream json = File.OpenRead(FilePath);
        List<Split> Splits = JsonSerializer.Deserialize<List<Split>>(json, _options);
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

        TemplateLoaded = true;
    }

    void OnOpenImageFolderBtnClicked(object sender, EventArgs e)
    {
        Process.Start("explorer.exe", ImagesPath);
    }
}

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Reflection;
using SpeedrunNotes.Popouts;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
	bool FirstAppear = true;

    bool ConnectionError = false;

    bool TemplateLoaded = false;

    int CurrentSplitIndex;

    string PreviousTitle;
    string PreviousLabel1;
    string PreviousLabel2;

    Socket soc;

    List<Split> SplitsInfo;

    // Bools for tracking if a popout is active or not
    bool NextSplitPopoutActive = false;
    bool SplitNote1PopoutActive = false;
    bool SplitNote2PopoutActive = false;

    Window NextSplitPopoutWindow;
    Window SplitNote1PopoutWindow;
    Window SplitNote2PopoutWindow;

    readonly string ImagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Images");
    readonly string TemplatesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Json-Templates");

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

    // Custom FileType to only show .json files in FilePicker
    static FilePickerFileType CustomFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json"} },
                });

    // Options for .json FilePicker
    PickOptions JsonFilepickerOptions = new()
    {
        PickerTitle = "Please select a comic file",
        FileTypes = CustomFileType,
    };

    public MainPage()
	{
		InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 720;
    }

    void OnMainPageLoaded(object sender, EventArgs e)
    {
        // When loaded, open the ConnectionPage
        Navigation.PushModalAsync(new ConnectionPage(ConnectionError));
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

                ConnectionError = false;

                // Start the timer / scheduled function calls
                InitTimer();
            }
			catch
			{
                // Update variable to show "ConnectionError" on ConnectionPage
                bool ConnectionError = true;

                // Bring back to ConnectionPage
                Navigation.PushModalAsync(new ConnectionPage(ConnectionError));
            }
        }

		FirstAppear = false;
        
        SplitNotes1Entry.Text = $"{SplitNoteLabel1.FontSize}";
        SplitNotes2Entry.Text = $"{SplitNoteLabel2.FontSize}";
    }

    public void InitTimer()
    {
        // Setup timer to send / recieve message with LiveSplit.Server every second
        System.Timers.Timer Timer = new(1000);
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
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

            // Send needed variables to each active popout
            if (NextSplitPopoutActive)
            {
                string NextSplitLabel = $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].SplitTitle}";
                string NextSplitImageFileLocation = Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex + 1].SplitImage);

                MessagingCenter.Send(this, "NextSplitLabel", NextSplitLabel);
                MessagingCenter.Send(this, "NextSplitImage", NextSplitImageFileLocation);
            }
            if (SplitNote1PopoutActive)
            {
                MessagingCenter.Send(this, "SplitNote1FontSize", SplitNoteLabel1.FontSize);
                MessagingCenter.Send(this, "SplitNote1Label", SplitsInfo[CurrentSplitIndex].SplitInfoText1);
            }
            if (SplitNote1PopoutActive)
            {
                MessagingCenter.Send(this, "SplitNote2FontSize", SplitNoteLabel2.FontSize);
                MessagingCenter.Send(this, "SplitNote1Label", SplitsInfo[CurrentSplitIndex].SplitInfoText2);
            }

            // Do UI update stuff, has to be on main thread cause Maui ig
            MainThread.BeginInvokeOnMainThread(UpdateUiElements);
        }
    }

    void UpdateUiElements()
    {
        // Get all active windows
        IReadOnlyList<Window> Windows = Application.Current.Windows;

        // Only update stuff if the element isnt "Popouted"
        if (!NextSplitPopoutActive)
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

                        string FileLocation = Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex + 1].SplitImage);

                        NextSplitImage.Source = ImageSource.FromFile(FileLocation);

                        PreviousTitle = NextSplitLabel.Text;
                    }
                }
                else if (SplitsInfo[CurrentSplitIndex + 1].SplitImage != "") // Only show ImageLoadError if an image is meant to show
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
        }
        // When NextSplitPopout is active, check if it has been disabled
        else
        {
            // If it has been disabled, re-enable the button and set NextSplitPopoutActive to false
            if (!Windows.Contains(NextSplitPopoutWindow))
            {
                PopoutNextSplitButton.IsEnabled = true;
                NextSplitPopoutActive = false;
                PreviousTitle = null; // Set to null so it has to be re-drawn
            }
        }

        // Only update stuff if the element isnt "Popouted"
        if (!SplitNote1PopoutActive)
        {
            try
            {
                if (File.Exists(Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex].SplitInfoImage1)))
                {
                    // Only redraw if something changed
                    if (PreviousLabel1 != SplitsInfo[CurrentSplitIndex].SplitInfoText1)
                    {
                        // Update notes for current split
                        SplitNoteLabel1.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText1;

                        string FileLocation = Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex].SplitInfoImage1);

                        SplitNoteImage1.Source = ImageSource.FromFile(FileLocation);

                        PreviousLabel1 = SplitNoteLabel1.Text;
                    }
                }
                else if (SplitsInfo[CurrentSplitIndex].SplitInfoImage1 != "") // Only show ImageLoadError if an image is meant to show
                {
                    // Only redraw if something changed
                    if (PreviousLabel1 != SplitsInfo[CurrentSplitIndex].SplitInfoText1)
                    {
                        // Update notes for current split
                        SplitNoteLabel1.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText1;
                        SplitNoteImage1.Source = "imageloadfail.png";

                        PreviousLabel1 = SplitNoteLabel1.Text;
                    }
                }
            }
            catch
            {

            }
        }
        // When SplitNote1Popout is active, check if it has been disabled
        else
        {
            // If it has been disabled, re-enable the button and set SplitNote1PopoutActive to false
            if (!Windows.Contains(NextSplitPopoutWindow))
            {
                PopoutSplitNote1Button.IsEnabled = true;
                SplitNote1PopoutActive = false;
                PreviousLabel1 = null; // Set to null so it has to be re-drawn
            }
        }

        // Only update stuff if the element isnt "Popouted"
        if (!SplitNote2PopoutActive)
        {
            try
            {
                if (File.Exists(Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex].SplitInfoImage2)))
                {
                    // Only redraw if something changed
                    if (PreviousLabel2 != SplitsInfo[CurrentSplitIndex].SplitInfoText2)
                    {
                        // Update notes for current split
                        SplitNoteLabel2.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText2;

                        string FileLocation = Path.Combine(ImagesPath, SplitsInfo[CurrentSplitIndex].SplitInfoImage2);

                        SplitNoteImage2.Source = ImageSource.FromFile(FileLocation);

                        PreviousLabel2 = SplitNoteLabel2.Text;
                    }
                }
                else if (SplitsInfo[CurrentSplitIndex].SplitInfoImage2 != "") // Only show ImageLoadError if an image is meant to show
                {
                    // Only redraw if something changed
                    if (PreviousLabel2 != SplitsInfo[CurrentSplitIndex].SplitInfoText2)
                    {
                        // Update notes for current split
                        SplitNoteLabel2.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText2;
                        SplitNoteImage2.Source = "imageloadfail.png";

                        PreviousLabel2 = SplitNoteLabel2.Text;
                    }
                }
            }
            catch
            {

            }
        }
        // When SplitNote2Popout is active, check if it has been disabled
        else
        {
            // If it has been disabled, re-enable the button and set SplitNote2PopoutActive to false
            if (!Windows.Contains(NextSplitPopoutWindow))
            {
                PopoutSplitNote2Button.IsEnabled = true;
                SplitNote2PopoutActive = false;
                PreviousLabel2 = null; // Set to null so it has to be re-drawn
            }
        }
    }

    public List<Split> JsonParse(string FilePath)
    {
        using FileStream json = File.OpenRead(FilePath);
        List<Split> Splits = JsonSerializer.Deserialize<List<Split>>(json, _options);
        return Splits;
    }

    void OnReconnectBtnClicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new ConnectionPage(ConnectionError));
	}

    async void OnLoadPresetBtnClicked(object sender, EventArgs e)
    {
        var File = await FilePicker.PickAsync(JsonFilepickerOptions);

        // Only do stuff to File if it succesfully picks a file
        if (File != null)
        {
            string FilePath = File.FullPath;
            SplitsInfo = JsonParse(FilePath);

            TemplateLoaded = true;

            // Enable all popout buttons since a template has been loaded
            PopoutNextSplitButton.IsEnabled = true;
            PopoutSplitNote1Button.IsEnabled = true;
            PopoutSplitNote2Button.IsEnabled = true;
        }
    }

    void OnOpenImageFolderBtnClicked(object sender, EventArgs e)
    {
        Process.Start("explorer.exe", ImagesPath);
    }

    void OnOpenTemplateFolderBtnClicked(object sender, EventArgs e)
    {
        Process.Start("explorer.exe", TemplatesPath);
    }

    void SplitNotes1FontSizeIncrease(object sender, EventArgs e)
    {
        SplitNoteLabel1.FontSize += 1;
        SplitNotes1Entry.Text = $"{SplitNoteLabel1.FontSize}";
    }

    void SplitNotes1FontSizeDecrease(object sender, EventArgs e)
    {
        SplitNoteLabel1.FontSize -= 1;
        SplitNotes1Entry.Text = $"{SplitNoteLabel1.FontSize}";
    }

    void SplitNotes2FontSizeIncrease(object sender, EventArgs e)
    {
        SplitNoteLabel2.FontSize += 1;
        SplitNotes2Entry.Text = $"{SplitNoteLabel2.FontSize}";
    }

    void SplitNotes2FontSizeDecrease(object sender, EventArgs e)
    {
        SplitNoteLabel2.FontSize -= 1;
        SplitNotes2Entry.Text = $"{SplitNoteLabel2.FontSize}";
    }

    void OnSplitNotesEntryTextChanged(object sender, EventArgs e)
    {
        // Make sure only numbers are entered
        var Builder = new StringBuilder();
        foreach (char ch in ((Entry)sender).Text)
        {
            if (char.IsDigit(ch))
            {
                Builder.Append(ch);
            }
        }
        ((Entry)sender).Text = Builder.ToString();

        // Update FontSize
        if (((Entry)sender).ClassId == "1")
        {
            SplitNoteLabel1.FontSize = int.Parse(Builder.ToString());
        }
        else
        {
            SplitNoteLabel2.FontSize = int.Parse(Builder.ToString());
        }
    }

    void OnPopoutNextSplitButtonClicked(object sender, EventArgs e)
    {
        // Disable the button so the user cant create more than one popouts
        PopoutNextSplitButton.IsEnabled = false;

        NextSplitPopoutActive = true;

        // Set text and image to not show
        NextSplitLabel.Text = "";
        NextSplitImage.Source = null;

        NextSplitPopoutWindow = new Window(new NextSplitPopout());

        Application.Current.OpenWindow(NextSplitPopoutWindow);
    }

    void OnPopoutSplitNote1ButtonClicked(object sender, EventArgs e)
    {
        // Disable the button so the user cant create more than one popouts
        PopoutSplitNote1Button.IsEnabled = false;

        SplitNote1PopoutActive = true;

        // Set text to not show
        SplitNoteLabel1.Text = "";

        SplitNote1PopoutWindow = new Window(new SplitNote1Popout());

        Application.Current.OpenWindow(SplitNote1PopoutWindow);
    }

    void OnPopoutSplitNote2ButtonClicked(object sender, EventArgs e)
    {
        // Disable the button so the user cant create more than one popouts
        PopoutSplitNote2Button.IsEnabled = false;

        SplitNote2PopoutActive = true;

        // Set text to not show
        SplitNoteLabel2.Text = "";

        SplitNote2PopoutWindow = new Window(new SplitNote1Popout());

        Application.Current.OpenWindow(SplitNote2PopoutWindow);
    }
}

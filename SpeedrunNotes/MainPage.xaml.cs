using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using SpeedrunNotes.Popouts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace SpeedrunNotes;

public partial class MainPage : ContentPage
{
    bool SidebarOut = true;

    bool TemplateLoaded = false;

    int CurrentSplitIndex = 0;
    int PreviousSplitIndex = 0;

    Socket Soc;

    List<Split> SplitsInfo;

    // Bools for tracking if a popout is active or not
    bool NextSplitPopoutActive = false;
    bool SplitNote1PopoutActive = false;
    bool SplitNote2PopoutActive = false;

    Window NextSplitPopoutWindow;
    Window SplitNote1PopoutWindow;
    Window SplitNote2PopoutWindow;

    string LoadedTemplatePath = "";

    private readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
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
        Window.Title = "SpeedrunNotes";
    }

    void OnMainPageLoaded(object sender, EventArgs e)
    {
        // When loaded, open the ConnectionPage
        Navigation.PushModalAsync(new ConnectionPage());

        // Attach a function to when closing the main window
        IReadOnlyList<Window> Windows = Application.Current.Windows;
        Windows[0].Destroying += WindowDestroying;
    }

    // When closing the MainPage window, exit application
    void WindowDestroying(object sender, EventArgs e)
    {
        // Get all active windows
        IReadOnlyList<Window> Windows = Application.Current.Windows;

        // Close all windows but one (Cant close the last one due to maui limitations)
        for (int i = 0; i < Windows.Count; i++)
        {
            Application.Current.CloseWindow(Windows[i]);
        }

        // Exit the application
        Application.Current.Quit();
    }

    void OnMainPageAppearing(object sender, EventArgs e)
	{
        try
        {
            // Setup Socket, IP and Port
            Soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress IP;

            // If IP is localhost, input the IP for localhost, eg 127.0.0.1
            if (Preferences.Default.Get("IP", "localhost") == "localhost")
            {
                IP = IPAddress.Parse("127.0.0.1");
            }
            else
            {
                IP = IPAddress.Parse(Preferences.Default.Get("IP", "localhost"));
            }

            IPEndPoint RemoteEP = new(IP, (Preferences.Default.Get("Port", 16834)));

            // Connect to livesplit.server
            Soc.Connect(RemoteEP);
        }
        catch
        {
            // Bring back to ConnectionPage
            Navigation.PushModalAsync(new ConnectionPage());
        }

        InitTimer();
    }

    public void InitTimer()
    {
        // Setup timer to send / recieve message with LiveSplit.Server every second
        System.Timers.Timer Timer = new(1000);
        Timer.Elapsed += GetLivesplitData;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private void GetLivesplitData(object sender, EventArgs e)
    {
        // So it only does update stuff if there is a TemplateLoaded
        if (!TemplateLoaded)
        {
            return;
        }

        // Send message to livesplit.server to check current split
        byte[] Message = Encoding.ASCII.GetBytes("getsplitindex\r\n");
        Soc.Send(Message);

        // Recieve message and "parse" it from computer-jargon -> readable string
        byte[] b = new byte[100];
        int k = Soc.Receive(b);
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

        // If selected split hasnt changed, theres no need to update anything
        if (PreviousSplitIndex == CurrentSplitIndex)
        {
            return;
        }

        // Send needed variables to each active popout
        if (NextSplitPopoutActive)
        {
            string NextSplitLabel = $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].Title}";
            string NextSplitImageFileLocation = Path.Combine(LoadedTemplatePath, SplitsInfo[CurrentSplitIndex + 1].ImageName);

            WeakReferenceMessenger.Default.Send(new NextSplitLabelMessage(NextSplitLabel));
            WeakReferenceMessenger.Default.Send(new NextSplitImageMessage(NextSplitImageFileLocation));
        }

        if (CurrentSplitIndex >= 0)
        {
            if (SplitNote1PopoutActive)
            {
                WeakReferenceMessenger.Default.Send(new SplitInfo1FontMessage((int)SplitNoteLabel1.FontSize));
                WeakReferenceMessenger.Default.Send(new SplitInfo1TextMessage(SplitsInfo[CurrentSplitIndex].InfoText1));
            }
            if (SplitNote2PopoutActive)
            {
                WeakReferenceMessenger.Default.Send(new SplitInfo2FontMessage((int)SplitNoteLabel2.FontSize));
                WeakReferenceMessenger.Default.Send(new SplitInfo2TextMessage(SplitsInfo[CurrentSplitIndex].InfoText2));
            }
        }

        // Do UI update stuff, has to be on main thread cause Maui ig
        MainThread.BeginInvokeOnMainThread(UpdateUiElements);

        PreviousSplitIndex = CurrentSplitIndex;
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
                string NewTitle = $"Next Split: {SplitsInfo[CurrentSplitIndex + 1].Title}";

                NextSplitLabel.Text = NewTitle;

                if (File.Exists(Path.Combine(LoadedTemplatePath, SplitsInfo[CurrentSplitIndex + 1].ImageName)))
                {
                    NextSplitImage.Source = $"{LoadedTemplatePath}/{SplitsInfo[CurrentSplitIndex + 1].ImageName}";
                }
                else if (SplitsInfo[CurrentSplitIndex + 1].ImageName != "") // Only show ImageLoadError if an image is meant to show
                {
                    NextSplitImage.Source = "imageloadfail.png";
                }
                else // If no image is meant to show
                {
                    NextSplitImage.Source = "";
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
            }
        }

        // Only update stuff if the element isnt "Popouted"
        if (!SplitNote1PopoutActive && CurrentSplitIndex >= 0)
        {
            try
            {
                string NewText = SplitsInfo[CurrentSplitIndex].InfoText1;

                SplitNoteLabel1.Text = NewText;

                if (File.Exists(Path.Combine(LoadedTemplatePath ,SplitsInfo[CurrentSplitIndex].InfoImageName1)))
                {
                    SplitNoteImage1.Source = $"{LoadedTemplatePath}/{SplitsInfo[CurrentSplitIndex].InfoImageName1}";
                }
                else if (SplitsInfo[CurrentSplitIndex].InfoImageName1 != "") // Only show ImageLoadError if an image is meant to show
                {
                    SplitNoteImage1.Source = "imageloadfail.png";
                }
                else // If no image is meant to show
                {
                    SplitNoteImage1.Source = "";
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
            if (!Windows.Contains(SplitNote1PopoutWindow))
            {
                PopoutSplitNote1Button.IsEnabled = true;
                SplitNote1PopoutActive = false;
            }
        }

        // Only update stuff if the element isnt "Popouted"
        if (!SplitNote2PopoutActive && CurrentSplitIndex >= 0)
        {
            try
            {
                string NewText = SplitsInfo[CurrentSplitIndex].InfoText2;

                SplitNoteLabel2.Text = NewText;

                if (File.Exists(Path.Combine(LoadedTemplatePath ,SplitsInfo[CurrentSplitIndex].InfoImageName2)))
                {
                    SplitNoteImage2.Source = $"{LoadedTemplatePath}/{SplitsInfo[CurrentSplitIndex].InfoImageName2}";
                }
                else if (SplitsInfo[CurrentSplitIndex].InfoImageName2 != "") // Only show ImageLoadError if an image is meant to show
                {
                    SplitNoteImage2.Source = "imageloadfail.png";
                }
                else // If no image is meant to show
                {
                    SplitNoteImage2.Source = "";
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
            if (!Windows.Contains(SplitNote2PopoutWindow))
            {
                PopoutSplitNote2Button.IsEnabled = true;
                SplitNote2PopoutActive = false;
            }
        }
    }

    public List<Split> JsonParse(string FilePath)
    {
        using FileStream Json = File.OpenRead(FilePath);
        List<Split> Splits = JsonSerializer.Deserialize<List<Split>>(Json, ReadOptions);
        return Splits;
    }

    void OnReconnectBtnClicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new ConnectionPage());
	}

    async void OnLoadPresetBtnClicked(object sender, EventArgs e)
    {
        var Result = await FolderPicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (Result.IsSuccessful)
        {
            LoadedTemplatePath = Result.Folder.Path;
            SplitsInfo = JsonParse($"{LoadedTemplatePath}/template.json");

            TemplateLoaded = true;

            // Enable all popout buttons since a template has been loaded
            PopoutNextSplitButton.IsEnabled = true;
            PopoutSplitNote1Button.IsEnabled = true;
            PopoutSplitNote2Button.IsEnabled = true;
        }
    }

    #region Font Size Stuff

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

    #endregion

    #region Popout Buttons

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

    #endregion

    private void ToggleSidebar(object sender, EventArgs e)
    {
        if (SidebarOut)
        {
            Sidebar.TranslateTo(250, 0);
            ToggleSidebarButton.Source = "open_sidebar.png";
        }
        else
        {
            Sidebar.TranslateTo(0, 0);
            ToggleSidebarButton.Source = "close_sidebar.png";
        }

        SidebarOut = !SidebarOut;
    }
}

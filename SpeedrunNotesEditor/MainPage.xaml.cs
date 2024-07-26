using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
	int SplitsAmount;

	// Lists for keeping track of each thing that will be saved in template.json file
	List<string> SplitNames = new();
	List<string> SplitImages = new();
    List<string> SplitNoteText1 = new();
    List<string> SplitNoteImage1 = new();
    List<string> SplitNoteText2 = new();
    List<string> SplitNoteImage2 = new();

    string PreviousElement;

    int CurrentSplitIndex = 0;

    bool SidebarOut = true;

    // Stuff for loading presets
    List<Split> SplitsInfo;

    // If null, then no file has been loaded
    string LoadedFilePath = null;

    // Bool for if currently loading template / creating template from splits to avoid dumb errors
    bool SettingTemplate = false;

    public class Split
    {
        public string SplitTitle { get; set; } = string.Empty;
        public string SplitImage { get; set; } = string.Empty;
        public string SplitInfoText1 { get; set; } = string.Empty;
        public string SplitInfoText2 { get; set; } = string.Empty;
        public string SplitInfoImage1 { get; set; } = string.Empty;
        public string SplitInfoImage2 { get; set; } = string.Empty;
    }

    private readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        Window.Title = "SpeedrunNotesEditor";
    }

    async void OnCreateFromTemplateClicked(object sender, EventArgs e)
	{
        var File = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (File != null)
        {
            SettingTemplate = true;

            LoadedFilePath = File.FullPath;
            SplitsInfo = JsonParse(LoadedFilePath);

            SplitsAmount = SplitsInfo.Count;

            // Clear Lists incase another template was previously loaded / created
            SplitNames.Clear();
            SplitImages.Clear();
            SplitNoteText1.Clear();
            SplitNoteImage1.Clear();
            SplitNoteText2.Clear();
            SplitNoteImage2.Clear();

            // Update corresponding lists
            for (int i = 0; i < SplitsInfo.Count; i++)
            {
                SplitNames.Add(SplitsInfo[i].SplitTitle);
                SplitImages.Add(SplitsInfo[i].SplitImage);
                SplitNoteText1.Add(SplitsInfo[i].SplitInfoText1);
                SplitNoteImage1.Add(SplitsInfo[i].SplitInfoImage1);
                SplitNoteText2.Add(SplitsInfo[i].SplitInfoText2);
                SplitNoteImage2.Add(SplitsInfo[i].SplitInfoImage2);
            }

            SettingTemplate = false;

            SplitSelector.ItemsSource = SplitNames;
        }
    }

    public List<Split> JsonParse(string FilePath)
    {
        using FileStream json = File.OpenRead(FilePath);
        List<Split> Splits = JsonSerializer.Deserialize<List<Split>>(json, _readOptions);
        return Splits;
    }

    void OnSaveTemplateClicked(object sender, EventArgs e)
    {
        // List to hold all the variables for writing into the json
        var TemplateVars = new List<Split>();

        // Make sure the List TemplateVars has all values set
        for (int i = 0; i < SplitsAmount; i++)
        {
            TemplateVars.Add(new Split() { SplitTitle = SplitNames[i], SplitImage = SplitImages[i], SplitInfoText1 = SplitNoteText1[i], SplitInfoImage1 = SplitNoteImage1[i], SplitInfoText2 = SplitNoteText2[i], SplitInfoImage2 = SplitNoteImage2[i] });
        }

        // Write to the loaded file
        JsonWrite(TemplateVars, LoadedFilePath);
    }

    public static void JsonWrite(object Obj, string FileName)
    {
        using var FileStream = File.Create(FileName);
        using var Utf8JsonWriter = new Utf8JsonWriter(FileStream);

        JsonSerializer.Serialize(Utf8JsonWriter, Obj, _writeOptions);
    }

    void OnSaveAsTemplateClicked(object sender, EventArgs e)
	{
        // Create a popup and pass through all important vars
        this.ShowPopup(new SaveTemplatePopup(SplitsAmount, SplitNames, SplitImages, SplitNoteText1, SplitNoteImage1, SplitNoteText2, SplitNoteImage2));
	}

    async void OnCreateFromSplitClicked(object sender, EventArgs e)
	{
		var SplitFile = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (SplitFile != null)
        {
            SettingTemplate = true;

            string FilePath = SplitFile.FullPath;

            // Clear Lists incase another template was previously loaded / created
            SplitNames.Clear();
            SplitImages.Clear();
            SplitNoteText1.Clear();
            SplitNoteImage1.Clear();
            SplitNoteText2.Clear();
            SplitNoteImage2.Clear();

            LssParse(FilePath);

            SettingTemplate = false;

            SplitSelector.ItemsSource = SplitNames;
        }
    }

	// Function for getting the relevant data out of the selected .lss file
	void LssParse(string FilePath)
	{
        // Reset SplitsAmount before re-calculating it
        SplitsAmount = 0;

        XmlTextReader Reader = new(FilePath);
		
		while (Reader.Read())
		{
			switch (Reader.NodeType)
			{
				case XmlNodeType.Element:
					if (Reader.Name == "Segment")
					{
                        SplitsAmount++;
                    }
					PreviousElement = Reader.Name;
					break;
				case XmlNodeType.Text:
					if (PreviousElement == "Name")
					{
						string NameText;

						// If the first char is "-", remove it
						if (Reader.Value.Substring(0, 1) == "-")
						{
							NameText = Reader.Value.Substring(1);
						}
                        // Remove "{texttexttext}" thing, the subsplit "chapter" name
						else if (Reader.Value.Substring(0, 1) == "{")
						{
							int Chars = 0;

							foreach (Char ch in Reader.Value)
							{
                                Chars++;
                                if (ch == '}')
                                {
                                    break;
                                }
                            }

							NameText = Reader.Value.Remove(0, Chars);
                        }
						// If it doesnt have any of the subsplits markers
						else
						{
							NameText = Reader.Value;
						}

						// Removes every space at front and end of the actual split name
						NameText = NameText.Trim(' ');

						// Add the cleaned name to the List
                        SplitNames.Add(NameText);
                    }
					break;
			}
		}

		// Make sure the following Lists are populated with as many items as SplitNames
		for (int i = 0; i < SplitNames.Count; i++)
		{
            SplitImages.Add(null);
            SplitNoteText1.Add(null);
            SplitNoteImage1.Add(null);
            SplitNoteText2.Add(null);
            SplitNoteImage2.Add(null);
        }
	}

	void UpdateTemplateDetailsViewer()
	{
        SplitNameEntry.Text = SplitNames[CurrentSplitIndex];
        SplitNote1TextEditor.Text = SplitNoteText1[CurrentSplitIndex];
        SplitNote2TextEditor.Text = SplitNoteText2[CurrentSplitIndex];

        // TODO: Set Images
    }

    void OnTextChanged(object sender, TextChangedEventArgs e)
	{
        if (!SettingTemplate)
        {
            switch (((VisualElement)sender).ClassId)
            {
                case "0":
                    SplitNames[CurrentSplitIndex] = e.NewTextValue;
                    break;
                case "1":
                    SplitNoteText1[CurrentSplitIndex] = e.NewTextValue;
                    break;
                case "2":
                    SplitNoteText2[CurrentSplitIndex] = e.NewTextValue;
                    break;
            }
        }
    }

    void OnImageSelectButtonClicked(object sender, EventArgs e)
    {
        if (!SettingTemplate)
        {
            var Image = FilePicker.Default.PickAsync(default);

            if (Image != null)
            {
                // TODO: STUFF
            }
        }
    }

    void OnTemplateEditingInfoButtonClicked(object sender, EventArgs e)
    {
        // Create a popup and pass through all important vars
        this.ShowPopup(new InfoPopup("template_editing_info.png"));
    }

    void OnSelectedIndexChanged(object sender, SelectionChangedEventArgs e)
    {
        string SplitName = e.CurrentSelection[0].ToString();
        CurrentSplitIndex = SplitNames.IndexOf(SplitName);
        UpdateTemplateDetailsViewer();
    }

    private void ToggleSidebar(object sender, EventArgs e)
    {
        if (SidebarOut)
        {
            Sidebar.TranslateTo(150, 0);
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

using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using CommunityToolkit.Maui.Storage;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
    int CurrentSplitIndex = 0;

    bool SidebarOut = true;

    // Stuff for loading presets
    public List<Split> SplitsInfo { get; set; } = new();

    // If null, then no file has been loaded
    string LoadedFilePath = null;

    // Bool for if currently loading template / creating template from splits to avoid dumb errors
    bool SettingTemplate = false;

    private bool _InputEnabled = false;
    private bool EnableInput
    {
        get
        {
            return _InputEnabled;
        }
        set
        {
            _InputEnabled = value;
            // Depending on if input is enabled all input-elements should be enabled/disabled to match this allowance
            SplitNameEntry.IsEnabled = value;
            SplitTitleImageButton.IsEnabled = value;
            SplitNote1TextEditor.IsEnabled = value;
            SplitNote1ImageButton.IsEnabled = value;
            SplitNote2TextEditor.IsEnabled = value;
            SplitNote2ImageButton.IsEnabled = value;
        }
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
        // Always dissalow input at start
        EnableInput = false;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 720;
        Window.Title = "SpeedrunNotesEditor";
    }

    async void OnLoadFromTemplateClicked(object sender, EventArgs e)
	{
        var Folder = await FolderPicker.PickAsync(default);

        // Only do stuff if succesfully picks a folder
        if (Folder != null)
        {
            SettingTemplate = true;

            LoadedFilePath = Folder.Folder.Path;
            SplitsInfo = JsonParse($"{LoadedFilePath}/template.json"); // Parse and load the json file

            SettingTemplate = false;

            SplitSelector.ItemsSource = SplitsInfo;
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
        // Write to the loaded file
        JsonWrite(SplitsInfo, LoadedFilePath);
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
        this.ShowPopup(new SaveTemplatePopup(SplitsInfo));
	}

    async void OnCreateFromSplitClicked(object sender, EventArgs e)
	{
		var SplitFile = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (SplitFile != null)
        {
            SettingTemplate = true;

            string FilePath = SplitFile.FullPath;

            LssParse(FilePath);

            SettingTemplate = false;
        }
    }

	// Function for getting the relevant data out of the selected .lss file
	void LssParse(string FilePath)
	{
        XmlTextReader Reader = new(FilePath);
        string PreviousElement = "";
		
		while (Reader.Read())
		{
			switch (Reader.NodeType)
			{
				case XmlNodeType.Element:
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

						// Add newly found split to SplitsInfo
                        SplitsInfo.Add(new Split() { SplitTitle = NameText });
                    }
					break;
			}
		}
	}

	void UpdateTemplateDetailsViewer()
	{
        SplitNameEntry.Text = SplitsInfo[CurrentSplitIndex].SplitTitle;
        SplitNote1TextEditor.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText1;
        SplitNote2TextEditor.Text = SplitsInfo[CurrentSplitIndex].SplitInfoText2;

        SplitTitleImage.Source = SplitsInfo[CurrentSplitIndex].SplitTitle;
        SplitNote1Image.Source = SplitsInfo[CurrentSplitIndex].SplitInfoImage1;
        SplitNote2Image.Source = SplitsInfo[CurrentSplitIndex].SplitInfoImage2;
    }

    void OnTextChanged(object sender, TextChangedEventArgs e)
	{
        if (!SettingTemplate)
        {
            switch (((VisualElement)sender).ClassId)
            {
                case "0":
                    SplitsInfo[CurrentSplitIndex].SplitTitle = e.NewTextValue;
                    break;
                case "1":
                    SplitsInfo[CurrentSplitIndex].SplitInfoText1 = e.NewTextValue;
                    break;
                case "2":
                    SplitsInfo[CurrentSplitIndex].SplitInfoText2 = e.NewTextValue;
                    break;
            }
        }
    }

    async void OnImageSelectButtonClicked(object sender, EventArgs e)
    {
        if (!SettingTemplate)
        {
            var Image = await FilePicker.Default.PickAsync(default);

            if (Image != null)
            {
                // Save selected images filepath so it later can be saved
                // Then set the image to be shown in editor window
                switch (((VisualElement)sender).ClassId)
                {
                    case "0":
                        SplitsInfo[CurrentSplitIndex].SplitImage = Image.FullPath;
                        SplitTitleImage.Source = ImageSource.FromFile(Image.FullPath);
                        break;
                    case "1":
                        SplitsInfo[CurrentSplitIndex].SplitInfoImage1 = Image.FullPath;
                        SplitNote1Image.Source = ImageSource.FromFile(Image.FullPath);
                        break;
                    case "2":
                        SplitsInfo[CurrentSplitIndex].SplitInfoImage2 = Image.FullPath;
                        SplitNote2Image.Source = ImageSource.FromFile(Image.FullPath);
                        break;
                }
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
        EnableInput = true;

        Split SelectedItem = (Split)e.CurrentSelection[0];
        string SplitName = SelectedItem.SplitTitle;

        // Get index that has the selected SplitName
        // Cant use IndexOf() cause reasons idk
        for (int i = 0; i < SplitsInfo.Count; i++)
        {
            if (SplitsInfo[i].SplitTitle == SplitName)
            {
                CurrentSplitIndex = i;
                break;
            }
        }
        
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

    private void SplitNameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}

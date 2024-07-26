using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
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

    async void OnLoadFromTemplateClicked(object sender, EventArgs e)
	{
        var File = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (File != null)
        {
            SettingTemplate = true;

            LoadedFilePath = File.FullPath;
            SplitsInfo = JsonParse(LoadedFilePath);

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

        // TODO: Set Images
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
}

using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Core.Primitives;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
    int CurrentSplitIndex = 0;

    bool SidebarOut = true;

    // Stuff for loading presets
    public List<Split> SplitsInfo { get; set; } = new();

    // If null, then no file has been loaded
    string LoadedFilePath = null;

    string TemplateName = "";

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

            TemplateNameEntry.Text = Folder.Folder.Name;
            TemplateNameEntry.IsEnabled = true;

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

    public static void JsonWrite(object Obj, string FileName)
    {
        using var FileStream = File.Create(FileName);
        using var Utf8JsonWriter = new Utf8JsonWriter(FileStream);

        JsonSerializer.Serialize(Utf8JsonWriter, Obj, _writeOptions);
    }

    async void OnSaveTemplateClicked(object sender, EventArgs e)
	{
        var Result = await FolderPicker.PickAsync(default);

        if (Result.IsSuccessful)
        {
            string SaveLocation = Result.Folder.Path;

            WriteToFolder(SaveLocation);
        }
	}

    void WriteToFolder(string SaveLocation)
    {
        FileSaveIndicator.SaveInProgress();

        try
        {
            string TemplateFolderPath = $"{SaveLocation}/{TemplateName}";

            // Create the template folder (unless it already exists)
            if (!Directory.Exists(TemplateFolderPath))
            {
                Directory.CreateDirectory(TemplateFolderPath);
            }

            // Write the template.json file
            using var FileStream = File.Create($"{TemplateFolderPath}/template.json");
            using var Utf8JsonWriter = new Utf8JsonWriter(FileStream);
            JsonSerializer.Serialize(Utf8JsonWriter, SplitsInfo, _writeOptions);

            // Move all images into the template folder
            for (int i = 0; i < SplitsInfo.Count; i++)
            {
                if (SplitsInfo[i].SplitImage != "")
                {
                    File.Copy(SplitsInfo[i].SplitImage, Path.Combine(TemplateFolderPath, Path.GetFileName(SplitsInfo[i].SplitImage)), true);
                }
                if (SplitsInfo[i].SplitInfoImage1 != "")
                {
                    File.Copy(SplitsInfo[i].SplitInfoImage1, Path.Combine(TemplateFolderPath, Path.GetFileName(SplitsInfo[i].SplitInfoImage1)), true);
                }
                if (SplitsInfo[i].SplitInfoImage2 != "")
                {
                    File.Copy(SplitsInfo[i].SplitInfoImage2, Path.Combine(TemplateFolderPath, Path.GetFileName(SplitsInfo[i].SplitInfoImage2)), true);
                }
            }

            FileSaveIndicator.SaveSucces();
        }
        catch
        {
            FileSaveIndicator.SaveFailed();
        }
    }

    void OnTemplateNameChanged(object sender, TextChangedEventArgs e)
    {
        TemplateName = e.NewTextValue;

        // Can only save if it has a name
        SaveButton.IsEnabled = TemplateName.Length > 0;
    }

    void OnCreateEmptyClicked(object sender, EventArgs e)
    {
        SettingTemplate = true;

        // Clear whatever is loaded
        SplitsInfo.Clear();
        SplitSelector.SelectedItem = null;
        EnableInput = false; // Since nothing is selected now

        SplitsInfo.Add(new Split() { SplitTitle = "Split 1" });

        SettingTemplate = false;
        TemplateNameEntry.IsEnabled = true;

        SplitSelector.ItemsSource = SplitsInfo;
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
            TemplateNameEntry.IsEnabled = true;

            SplitSelector.ItemsSource = SplitsInfo;
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

        SplitTitleImage.Source = SplitsInfo[CurrentSplitIndex].SplitImage;
        SplitNote1Image.Source = SplitsInfo[CurrentSplitIndex].SplitInfoImage1;
        SplitNote2Image.Source = SplitsInfo[CurrentSplitIndex].SplitInfoImage2;
    }

    void ClearTemplateDetailsViewer()
    {
        SplitNameEntry.Text = "";
        SplitNote1TextEditor.Text = "";
        SplitNote2TextEditor.Text = "";

        SplitTitleImage.Source = "";
        SplitNote1Image.Source = "";
        SplitNote2Image.Source = "";
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
        // If we are selecting something
        if (e.CurrentSelection.Count > 0)
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
        else
        {
            ClearTemplateDetailsViewer();
        }
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

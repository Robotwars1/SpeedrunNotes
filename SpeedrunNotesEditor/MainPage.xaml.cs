using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
    int CurrentSplitIndex = 0;

    bool SidebarOut = true;

    // Stuff for loading presets
    private List<Split> SplitsInfo = new();
    public ObservableCollection<string> SplitTitles { get; set; } = new();

    // If null, then no file has been loaded
    string LoadedFilePath = null;

    string TemplateName = "";

    // Bool for if currently loading template / creating template from splits to avoid dumb errors
    bool SettingTemplate = false;

    List<ImageFilePaths> ImagePaths = new();

    public class ImageFilePaths
    {
        public string TitleImageName { get; set; } = string.Empty;
        public string Notes1ImageName { get; set; } = string.Empty;
        public string Notes2ImageName { get; set; } = string.Empty;
    }

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
        BindingContext = this;
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
        var Result = await FolderPicker.PickAsync(default);

        // Only do stuff if succesfully picks a folder
        if (Result.IsSuccessful)
        {
            SettingTemplate = true;

            ClearLoadedFile();

            LoadedFilePath = Result.Folder.Path;
            SplitsInfo = JsonParse($"{LoadedFilePath}/template.json"); // Parse and load the json file

            for (int i = 0; i < SplitsInfo.Count; i++)
            {
                SplitTitles.Add(SplitsInfo[i].Title); // Dumb workaround to make splits show in tabbar
                ImagePaths.Add(new ImageFilePaths());
            }

            TemplateNameEntry.Text = Result.Folder.Name;
            TemplateNameEntry.IsEnabled = true;

            SettingTemplate = false;
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
                if (SplitsInfo[i].ImageName != "")
                {
                    File.Copy(ImagePaths[i].TitleImageName, Path.Combine(TemplateFolderPath, Path.GetFileName(ImagePaths[i].TitleImageName)), true);
                }
                if (SplitsInfo[i].InfoImageName1 != "")
                {
                    File.Copy(ImagePaths[i].Notes1ImageName, Path.Combine(TemplateFolderPath, Path.GetFileName(ImagePaths[i].Notes1ImageName)), true);
                }
                if (SplitsInfo[i].InfoImageName2 != "")
                {
                    File.Copy(ImagePaths[i].Notes2ImageName, Path.Combine(TemplateFolderPath, Path.GetFileName(ImagePaths[i].Notes2ImageName)), true);
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

    void ClearLoadedFile()
    {
        SplitsInfo.Clear();
        SplitTitles.Clear();
        ImagePaths.Clear();
        SplitSelector.SelectedItem = null;
        ClearTemplateDetailsViewer();
        EnableInput = false; // Since nothing is selected now
    }

    void OnCreateEmptyClicked(object sender, EventArgs e)
    {
        SettingTemplate = true;

        ClearLoadedFile();

        SplitsInfo.Add(new Split() { Title = "Split 1" });
        SplitTitles.Add("Split 1");
        ImagePaths.Add(new ImageFilePaths());

        SettingTemplate = false;
        TemplateNameEntry.IsEnabled = true;
    }

    async void OnCreateFromSplitClicked(object sender, EventArgs e)
	{
		var SplitFile = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (SplitFile != null)
        {
            SettingTemplate = true;

            ClearLoadedFile();

            string FilePath = SplitFile.FullPath;

            LssParse(FilePath);

            SettingTemplate = false;
            TemplateNameEntry.IsEnabled = true;
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
						string NameText = "";

						// If the first char is "-", remove it
						if (Reader.Value.Substring(0, 1) == "-")
						{
							NameText = Reader.Value.Substring(1);
						}
                        // Remove "{texttexttext}" thing, the subsplit "chapter" name
						else if (Reader.Value.Substring(0, 1) == "{")
						{
							int Chars = 0;

							foreach (char ch in Reader.Value)
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
                        SplitsInfo.Add(new Split() { Title = NameText });
                        SplitTitles.Add(NameText);
                        Debug.WriteLine($"{NameText} {SplitsInfo.Count}");
                        ImagePaths.Add(new ImageFilePaths());
                    }
					break;
			}
		}
    }

	void UpdateTemplateDetailsViewer()
	{
        SplitNameEntry.Text = SplitsInfo[CurrentSplitIndex].Title;
        SplitNote1TextEditor.Text = SplitsInfo[CurrentSplitIndex].InfoText1;
        SplitNote2TextEditor.Text = SplitsInfo[CurrentSplitIndex].InfoText2;

        SplitTitleImage.Source = ImagePaths[CurrentSplitIndex].TitleImageName;
        SplitNote1Image.Source = ImagePaths[CurrentSplitIndex].Notes1ImageName;
        SplitNote2Image.Source = ImagePaths[CurrentSplitIndex].Notes2ImageName;
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
                    if (SplitTitles[CurrentSplitIndex] == e.NewTextValue)
                    {
                        return;
                    }

                    SplitsInfo[CurrentSplitIndex].Title = e.NewTextValue;
                    SplitTitles[CurrentSplitIndex] = e.NewTextValue;
                    SplitSelector.SelectedItem = SplitTitles[CurrentSplitIndex];
                    break;
                case "1":
                    SplitsInfo[CurrentSplitIndex].InfoText1 = e.NewTextValue;
                    break;
                case "2":
                    SplitsInfo[CurrentSplitIndex].InfoText2 = e.NewTextValue;
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
                        SplitsInfo[CurrentSplitIndex].ImageName = Image.FileName;
                        ImagePaths[CurrentSplitIndex].TitleImageName = Image.FullPath;
                        SplitTitleImage.Source = ImageSource.FromFile(Image.FullPath);
                        break;
                    case "1":
                        SplitsInfo[CurrentSplitIndex].InfoImageName1 = Image.FileName;
                        ImagePaths[CurrentSplitIndex].Notes1ImageName = Image.FullPath;
                        SplitNote1Image.Source = ImageSource.FromFile(Image.FullPath);
                        break;
                    case "2":
                        SplitsInfo[CurrentSplitIndex].InfoImageName2 = Image.FileName;
                        ImagePaths[CurrentSplitIndex].Notes2ImageName = Image.FullPath;
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

            string SelectedItem = (string)e.CurrentSelection[0];

            // Get index that has the selected SplitName
            // Cant use IndexOf() cause reasons idk
            for (int i = 0; i < SplitTitles.Count; i++)
            {
                if (SplitTitles[i] == SelectedItem)
                {
                    CurrentSplitIndex = i;
                    break;
                }
            }

            UpdateTemplateDetailsViewer();
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
}

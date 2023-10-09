using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpeedrunNotesEditor;

public partial class SaveTemplatePopup : Popup
{
    string FileName;
    string FilePath;

    int SplitsAmount;

    // Lists for keeping track of each thing that will be saved in template.json file
    List<string> SplitNames;
    List<string> SplitImages;
    List<string> SplitNoteText1;
    List<string> SplitNoteImage1;
    List<string> SplitNoteText2;
    List<string> SplitNoteImage2;

    public class Split
    {
        public string SplitTitle { get; set; } = string.Empty;
        public string SplitImage { get; set; } = string.Empty;
        public string SplitInfoText1 { get; set; } = string.Empty;
        public string SplitInfoText2 { get; set; } = string.Empty;
        public string SplitInfoImage1 { get; set; } = string.Empty;
        public string SplitInfoImage2 { get; set; } = string.Empty;
    }

    public SaveTemplatePopup(int splitsAmount, List<string> splitNames, List<string> splitImages, List<string> splitNoteText1, List<string> splitNoteImage1, List<string> splitNoteText2, List<string> splitNoteImage2)
    {
        InitializeComponent();

        // (width, height)
        Size = new Size(400, 250);

        // Make sure all vars are assigned
        SplitsAmount = splitsAmount;
        SplitNames = splitNames;
        SplitImages = splitImages;
        SplitNoteText1 = splitNoteText1;
        SplitNoteImage1 = splitNoteImage1;
        SplitNoteText2 = splitNoteText2;
        SplitNoteImage2 = splitNoteImage2;
    }

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    void OnFileNameChanged(object sender, TextChangedEventArgs e)
    {
        FileName = e.NewTextValue;

        // If last 5 chars != .json, add it
        if (FileName.Length > 4)
        {
            if (FileName.Substring(FileName.Length - 5) != ".json")
            {
                var builder = new StringBuilder();

                foreach (var Char in FileName)
                {
                    builder.Append(Char);
                }

                builder.Append(".json");

                FileName = builder.ToString();
            }
        }
    }

    async void OnFileLocationButtonClicked(object sender, EventArgs e)
    {
        var Folder = await FolderPicker.Default.PickAsync(default);

        if (Folder != null)
        {
            FilePath = Folder.Folder.Path;
        }
    }

    async void OnSaveFileButtonClicked(object sender, EventArgs e)
    {
        var TemplateVars = new List<Split>();
        var File = Path.Combine(FilePath, FileName); // NOT WORKING

        // Make sure the List TemplateVars has all values set
        for (int i = 0; i < SplitsAmount; i++)
        {
            TemplateVars.Add(new Split() { SplitTitle = SplitNames[i], SplitImage = SplitImages[i], SplitInfoText1 = SplitNoteText1[i], SplitInfoImage1 = SplitNoteImage1[i], SplitInfoText2 = SplitNoteText2[i], SplitInfoImage2 = SplitNoteImage2[i] });
        }

        jsonWrite(TemplateVars, File);

        await CloseAsync();
    }

    public static void jsonWrite(object Obj, string FileName)
    {
        using var FileStream = File.Create(FileName);
        using var Utf8JsonWriter = new Utf8JsonWriter(FileStream);

        JsonSerializer.Serialize(Utf8JsonWriter, Obj, _writeOptions);
    }
}

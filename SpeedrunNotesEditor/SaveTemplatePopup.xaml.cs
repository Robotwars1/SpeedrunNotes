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

    List<MainPage.Split> SplitsInfo;

    public SaveTemplatePopup(List<MainPage.Split> splitsInfo)
    {
        InitializeComponent();

        // (width, height)
        Size = new Size(300, 225);

        // Make sure all vars are assigned
        SplitsInfo = splitsInfo;
    }

    void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close();
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

            SaveButton.IsEnabled = true;
        }
    }

    async void OnSaveFileButtonClicked(object sender, EventArgs e)
    {
        var File = Path.Combine(FilePath, FileName);

        JsonWrite(SplitsInfo, File);

        await CloseAsync();
    }

    public static void JsonWrite(object Obj, string FileName)
    {
        using var FileStream = File.Create(FileName);
        using var Utf8JsonWriter = new Utf8JsonWriter(FileStream);

        JsonSerializer.Serialize(Utf8JsonWriter, Obj, _writeOptions);
    }
}

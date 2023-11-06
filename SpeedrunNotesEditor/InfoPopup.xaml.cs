using CommunityToolkit.Maui.Views;

namespace SpeedrunNotesEditor;

public partial class InfoPopup : Popup
{
    public InfoPopup(string ImageSource)
    {
        InitializeComponent();

        // (width, height)
        Size = new Size(711, 420);

        MainImage.Source = ImageSource;
    }

    void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close();
    }
}

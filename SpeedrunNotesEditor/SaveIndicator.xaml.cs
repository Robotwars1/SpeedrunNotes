namespace SpeedrunNotesEditor;

public partial class SaveIndicator : ContentView
{
	public SaveIndicator()
	{
		InitializeComponent();
	}

	public void SaveInProgress()
	{
		IsVisible = true;
		LabelFrame.BackgroundColor = Color.FromArgb("#404040");
        LabelFrame.BorderColor = Color.FromArgb("#404040");
        TextLabel.Text = "Saving";
	}

	public async void SaveSucces()
	{
        LabelFrame.BackgroundColor = Color.FromArgb("#0c9146");
        LabelFrame.BorderColor = Color.FromArgb("#0c9146");
        TextLabel.Text = "Saved File";

		await Task.Delay(5000);
		IsVisible = false;
    }

	public async void SaveFailed()
	{
        LabelFrame.BackgroundColor = Color.FromArgb("#dc2328");
        LabelFrame.BorderColor = Color.FromArgb("#dc2328");
        TextLabel.Text = "Failed to Save File";

        await Task.Delay(5000);
        IsVisible = false;
    }
}

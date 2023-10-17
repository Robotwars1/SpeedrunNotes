using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;
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

	// 0 == Split Info
	// 1 == Split Notes 1
	// 2 == Split Notes 2
	int ViewMode = 0;

    public class DetailsViewer
	{
		public string IndexLabel { get; set; }
		public string DetailsLabelText { get; set; }
		public string TextEntryId { get; set; }
		public string DetailsImageUrl { get; set; }
		public string ImageEntryId { get; set; }
	}

    // Creates Collections of each class to populate each CollectionView
    ObservableCollection<DetailsViewer> templateDetailsViewer = new();
    public ObservableCollection<DetailsViewer> TemplateDetailsViewer { get { return templateDetailsViewer; } }

    // Stuff for loading presets
    List<Split> SplitsInfo;

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

    public MainPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 720;
    }

    async void OnLoadTemplateClicked(object sender, EventArgs e)
	{
        var File = await FilePicker.PickAsync(default);

        // Only do stuff to File if it succesfully picks a file
        if (File != null)
        {
            string FilePath = File.FullPath;
            SplitsInfo = JsonParse(FilePath);

            SplitsAmount = SplitsInfo.Count;

            /*
            // Clear Lists
            SplitNames.Clear();
            SplitImages.Clear();
            SplitNoteText1.Clear();
            SplitNoteImage1.Clear();
            SplitNoteText2.Clear();
            SplitNoteImage2.Clear();
            */

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

            UpdateTemplateDetailsViewer();
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
        // Create a popup and pass through all important vars
        this.ShowPopup(new SaveTemplatePopup(SplitsAmount, SplitNames, SplitImages, SplitNoteText1, SplitNoteImage1, SplitNoteText2, SplitNoteImage2));
	}

    async void OnSelectSplitPresetClicked(object sender, EventArgs e)
	{
		var SplitFile = await FilePicker.PickAsync(default);

		string FilePath = SplitFile.FullPath;

		LssParse(FilePath);

		UpdateTemplateDetailsViewer();
    }

	// Function for getting the relevant data out of the selected .lss file
	void LssParse(string FilePath)
	{
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
		templateDetailsViewer.Clear();

		switch (ViewMode)
		{
            // Split Info
            case 0:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), TextEntryId = i.ToString(), ImageEntryId = i.ToString(), DetailsLabelText = SplitNames[i], DetailsImageUrl = SplitImages[i] });
                }
                break;
            // Split Notes 1
            case 1:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), TextEntryId = i.ToString(), ImageEntryId = i.ToString(), DetailsLabelText = SplitNoteText1[i], DetailsImageUrl = SplitNoteImage1[i] });
                }
                break;
            // Split Notes 2
            case 2:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), TextEntryId = i.ToString(), ImageEntryId = i.ToString(), DetailsLabelText = SplitNoteText2[i], DetailsImageUrl = SplitNoteImage2[i] });
                }
                break;
		}

        TemplateDetailsViewerCollectionView.ItemsSource = templateDetailsViewer;
    }

	void OnSplitInfoClicked(object sender, EventArgs e)
	{
		ViewMode = 0;

		UpdateTemplateDetailsViewer();
	}

    void OnSplitNotes1Clicked(object sender, EventArgs e)
    {
		ViewMode = 1;

        UpdateTemplateDetailsViewer();
    }

    void OnSplitNotes2Clicked(object sender, EventArgs e)
    {
		ViewMode = 2;

        UpdateTemplateDetailsViewer();
    }

	void UpdateDetailsText(object sender, TextChangedEventArgs e)
	{
        switch (ViewMode)
        {
            case 0:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].TextEntryId)
                    {
                        SplitNames[i] = e.NewTextValue;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].TextEntryId)
                    {
                        SplitNoteText1[i] = e.NewTextValue;
                    }
                }
                break;
            case 2:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].TextEntryId)
                    {
                        SplitNoteText2[i] = e.NewTextValue;
                    }
                }
                break;
        }
    }

    void UpdateDetailsImage(object sender, TextChangedEventArgs e)
    {
        switch (ViewMode)
        {
            case 0:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].ImageEntryId)
                    {
                        SplitImages[i] = e.NewTextValue;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].ImageEntryId)
                    {
                        SplitNoteImage1[i] = e.NewTextValue;
                    }
                }
                break;
            case 2:
                for (int i = 0; i < SplitsAmount; i++)
                {
                    if (((Entry)sender).ClassId == templateDetailsViewer[i].ImageEntryId)
                    {
                        SplitNoteImage2[i] = e.NewTextValue;
                    }
                }
                break;
        }
    }
}

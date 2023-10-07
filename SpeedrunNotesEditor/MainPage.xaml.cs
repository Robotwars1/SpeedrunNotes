﻿using System.Collections.ObjectModel;
using System.Xml;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
	int SplitsAmount;

	// Lists for keeping track of each thing that will be saved in template.json file
	List<string> SplitNames = new List<string>();
	List<string> SplitImages = new List<string>();
    List<string> SplitNoteText1 = new List<string>();
    List<string> SplitNoteImage1 = new List<string>();
    List<string> SplitNoteText2 = new List<string>();
    List<string> SplitNoteImage2 = new List<string>();

    string PreviousElement;

	// 0 == Split Info
	// 1 == Split Notes 1
	// 2 == Split Notes 2
	int ViewMode = 0;

	public class DetailsViewer
	{
		public string IndexLabel { get; set; }
		public string DetailsLabelText { get; set; }
		public string DetailsImageUrl { get; set; }
	}

    // Creates Collections of each class to populate each CollectionView
    ObservableCollection<DetailsViewer> templateDetailsViewer = new ObservableCollection<DetailsViewer>();
    public ObservableCollection<DetailsViewer> TemplateDetailsViewer { get { return templateDetailsViewer; } }

    public MainPage()
	{
		InitializeComponent();
	}

	void OnLoadTemplateClicked(object sender, EventArgs e)
	{

	}

	void OnCreateTemplateClicked(object sender, EventArgs e)
	{

	}

	async void OnSelectSplitPresetClicked(object sender, EventArgs e)
	{
		var SplitFile = await FilePicker.PickAsync(default);

		string FilePath = SplitFile.FullPath;

		lssParse(FilePath);

		UpdateTemplateDetailsViewer();
    }

	// Function for getting the relevant data out of the selected .lss file
	void lssParse(string FilePath)
	{
		XmlTextReader Reader = new XmlTextReader(FilePath);
		
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
						string NameText = null;

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
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), DetailsLabelText = SplitNames[i], DetailsImageUrl = SplitImages[i] });
                }
                break;
            // Split Notes 1
            case 1:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), DetailsLabelText = SplitNoteText1[i], DetailsImageUrl = SplitNoteImage1[i] });
                }
                break;
            // Split Notes 2
            case 2:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), DetailsLabelText = SplitNoteText2[i], DetailsImageUrl = SplitNoteImage2[i] });
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
}

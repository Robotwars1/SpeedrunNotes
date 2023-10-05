using System.Collections.ObjectModel;
using System.Xml;
using Windows.ApplicationModel.Activation;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
	int SplitsAmount;
	List<string> SplitNames = new List<string>();

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
						SplitNames.Add(Reader.Value);
					}
					break;
			}
		}
	}

	void UpdateTemplateDetailsViewer()
	{
		switch (ViewMode)
		{
            // Split Info
            case 0:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString(), DetailsLabelText = SplitNames[i] /*VALUES*/ });
                }
                break;
            // Split Notes 1
            case 1:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString() /*VALUES*/ });
                }
                break;
            // Split Notes 2
            case 2:
                // Add a "line" for CollectionView, corresponding to how many splits there are
                for (int i = 0; i < SplitsAmount; i++)
                {
                    templateDetailsViewer.Add(new DetailsViewer() { IndexLabel = (i + 1).ToString() /*VALUES*/ });
                }
                break;
		}

        TemplateDetailsViewerCollectionView.ItemsSource = templateDetailsViewer;
    }
}

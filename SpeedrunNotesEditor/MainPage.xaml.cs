﻿using System.Xml;

namespace SpeedrunNotesEditor;

public partial class MainPage : ContentPage
{
	int SplitsAmount;
	List<string> SplitNames;

    string PreviousElement;

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
}

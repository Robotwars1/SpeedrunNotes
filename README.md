
<div align="center">

### [Installation](#installation-1)

<br>

### SpeedrunNotes
### [Overview](#speedrunnotes-overview) | [Using](#speedrunnotes-using)

<br>

### SpeedrunNotesEditor
### [Overview](#speedrunnoteseditor-overview) | [Using](#speedrunnoteseditor-using)

<br>

### Technical Details
### [.JSON Template](#json-template) | [Python Scripts](#python-scripts)

<br>

</div>

# Installation
To install SpeedrunNotes and SpeedrunNotesEditor, head to releases and find the latest one. 
Press "Assets" under the latest release and then download the .zip file at the top of the list. 
When its finished, extract the contents of the folder and launch the .exe and you are set!

**IMPORTANT:** You need LiveSplit.Server in your split layout for this program to work.
If you don't have it added to your layout, head over to [LiveSplit.Servers github]("https://github.com/LiveSplit/LiveSplit.Server#install") and follow the installation instructions there.

# SpeedrunNotes Overview
SpeedrunNotes is a program written in C# and Xaml with the help of .Net Maui. By communicating with [LiveSplit.Server](https://github.com/LiveSplit/LiveSplit.Server) it gets information about your current split and shows any notes
needed for said split while also showing which split is next, providing that crusual information for users who elect to play with splits hidden.

# SpeedrunNotes Using
When starting SpeedrunNotes, you will be greeted with 2 input boxes allowing you to input the local IP and Port used to connect to LiveSplit.Server. Without changing anything these are the defaults provided by LiveSplit.Server and 
should work but if it doesnt work make sure it matches the defaults.

**Important: BEFORE** pressing the connect button, make sure you have started the server (Rightclick -> Control -> Start Server)

When you have connected to LiveSplit.Server, make sure to select a .json template file corresponding to your current splits and make sure all the required images are available in the images folder (can be opened via the open image folder button).
Otherwise the missing images will be replaced with an error image which provides 0 help.

# SpeedrunNotesEditor Overview
SpeedrunNotesEditor is a program that lets you create a templates for SpeedrunNotes. Its made to be simple to use and has the ability to automate parts of the template creating.

# SpeedrunNotesEditor Using
When starting the program there are 2 primary ways to use it. Either you click "Load Template" to edit a currently existing template or "Select Split" to automaticly create a new template with split names automaticly put into the template.

At the bottom of the screen there are 3 buttons which allow you to switch which part of the template you are editing. Currently these are: split preview, split notes row 1, split notes row 2.

When you have finished editing these, press the "Save Template" button and you will have a template that works for your splits in SpeedrunNotes.

# .JSON Template
All split information is saved in .json files to be read and parsed for the program.
Below is an example of how these .json files are structured

```json
[
    {
        "splitTitle":"",
        "splitImage":"",
        "splitInfoText1":"",
        "splitInfoText2":"",
        "splitInfoImage1":"",
        "splitInfoImage2":""
    }
]
```
And a chart of what type each variable is

| Variable        | Type   | Property  |
| --------------- | ------ | --------- |
| splitTitle      | String | Title     |
| splitImage      | String | Image URL |
| splitInfoText1  | String | Text      |
| splitInfoText2  | String | Text      |
| splitInfoImage1 | String | Image URL |
| splitInfoImage2 | String | Image URL |

## Template Creation
For creating a template there are 2 primary ways, either via editing a .json file direcly through a code editor or by using SpeedrunNotesEditor. For writing a template through code, make sure to copy the template shown above. If using SpeedrunNotesEditor then follow [usage instructions](#speedrunnotes-using).

# Python Scripts

## ReleaseBuilder.py
ReleaseBuilder is a custom-made Python script that automates the process of building each project and packing the release. Just by inputting the release name it does the rest for you, with the only exception being you have to make a .zip by yourself.

**IMPORTANT:** For running it, make sure to install [Python 3.11 or later](https://www.python.org/downloads/) and run the following command in the Command Prompt.
```
python -m pip install --upgrade pywin32
```

## FileMover.py
FileMover is a Python script that can move every image and template file from an old version to a new version to make updating less of a pain.

**IMPORTANT:** For running it, make sure to install [Python 3.11 or later](https://www.python.org/downloads/)

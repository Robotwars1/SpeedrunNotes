
<div align="center">

### SpeedrunNotes
### [Overview](#speedrunnotes-overview) | [Installation](#speedrunnotes-installation) | [Using](#speedrunnotes-using)

<br>

### SpeedrunNotesEditor
### [Overview](#speedrunnoteseditor-overview) | [Installation](#speedrunnoteseditor-installation) | [Using](#speedrunnoteseditor-using)

<br>

### Technical Details
### [.JSON Template](#json-template)

</div>

# SpeedrunNotes Overview
SpeedrunNotes is a program written in C# and Xaml with the help of .Net Maui. By communicating with [LiveSplit.Server](https://github.com/LiveSplit/LiveSplit.Server) it gets information about your current split and shows any notes
needed for said split while also showing which split is next, providing that crusual information for users who elect to play with splits hidden.

# SpeedrunNotes Installation
**TBD**

# SpeedrunNotes Using
When starting SpeedrunNotes, you will be greeted with 2 input boxes allowing you to input the local IP and Port used to connect to LiveSplit.Server. Without changing anything these are the defaults provided by LiveSplit.Server and 
should work but if it doesnt work make sure it matches the defaults.

**Important: BEFORE** pressing the connect button, make sure you have started the server (Rightclick -> Control -> Start Server)

When you have connected to LiveSplit.Server, make sure to select a .json template file corresponding to your current splits and make sure all the required images are available in the images folder (can be opened via the open image folder button).
Otherwise the missing images will be replaced with an error image which provides 0 help.

# SpeedrunNotesEditor Overview
SpeedrunNotesEditor is the working title of the planned editor that will make it easy to create .json templates by easily inputing data from livesplit. It will also let the user attach given Notes text and images + allow automatic moving
to SpeedrunNotes folders

# SpeedrunNotesEditor Installation
**TBD**

# SpeedrunNotesEditor Using
**TBD**

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
As it currently stands, if you want to make a new preset everything has to be done by editing a pure .json file. To do this, base it of the Template.json and add a new instance of the block ( {} - brackets included ).
In the future, a dedicated editor program will be developed and put into this repository



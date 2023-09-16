# SpeedrunNotes
SpeedrunNotes is a program written in C# and Xaml with the help of .Net Maui. By communicating with [LiveSplit.Server](https://github.com/LiveSplit/LiveSplit.Server) it gets information about your current split and shows any notes
needed for said split while also showing which split is next, providing that crusual information for users who elect to play with splits hidden.

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

| Type   | Var Name        |
| -------| --------------- |
| String | splitTitle      |
| String | splitImage      |
| String | splitInfoText1  |
| String | splitInfoText2  |
| String | splitInfoImage1 |
| String | splitInfoImage2 |

# Template Creation
As it currently stands, if you want to make a new preset everything has to be done by editing a pure .json file. To do this, base it of the Template.json and add a new instance of the block ( {} - brackets included ).
In the future, a dedicated editor program will be developed and put into this repository

# SpeedrunNotesEditor
SpeedrunNotesEditor is the working title of the planned editor that will make it easy to create .json templates by easily inputing data from livesplit. It will also let the user attach given Notes text and images + allow automatic moving
to SpeedrunNotes folders

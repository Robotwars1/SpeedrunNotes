import shutil
import os

def MoveImages():
    print("Moving Images")

    # Move images to new folder
    shutil.copytree(OldImagePath, NewImagePath)

def MoveTemplates():
    print("Moving Templates")

    # Move templates to new folder
    shutil.copytree(OldTemplatePath, NewTemplatePath)

def MoveFiles():
    MoveImages()

    MoveTemplates()

# Write out header
print("___________________________________")
print("\nFileMover v1.0.0")
print("___________________________________")

# Get variables
print("\nInput the full filepath of the old versions folder\nThis folder is called v1.x.x and contains both SpeedrunNotes and SpeedrunNotesEditor")
print("To get this filepath, open the folder and click the field in the top that is showing every parent folder\nCopy the full path and paste it in below")
OldReleaseFilepath = input("Filepath: ")

# Set the rest of the variables
ScriptPath = os.getcwd()

OldImagePath = f"{OldReleaseFilepath}\SpeedrunNotes\Images"
NewImagePath = f"{ScriptPath}\SpeedrunNotes\Images"

OldTemplatePath = f"{OldReleaseFilepath}\SpeedrunNotes\Json-Templates"
NewTemplatePath = f"{ScriptPath}\SpeedrunNotes\Json-Templates"

# Write out more stuff
#print("\nProjects to build: \n  -SpeedrunNotes \n  -SpeedrunNotesEditor")
#print(f"\nBuild destination: {ScriptPath}\{ReleaseFolder}")
AAA = input("\nPress Enter to start building the Release")
print("___________________________________")

MoveFiles()

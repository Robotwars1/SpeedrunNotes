import shutil
import subprocess
import os
import time
from win32com.client import Dispatch

def SetupRelease():
    print("Setting up Release")

    # Create a folder for the new release
    shutil.copytree('Release-Template', ReleaseFolder)

def BuildProjects():
    print("Building Projects")

    # Build each project
    for ProjectPath in ProjectsPath:
        subprocess.call([MSBuild, ProjectPath, Arg1])

def MoveBuildResults():
    print("Moving Build Results")

    # Copy each build folder to the created release folder
    for i in range(2):
        shutil.copytree(BuildSourcePaths[i], BuildDestinationPaths[i])

def CreateShortcuts():
    print("Creating Shortcuts")

    # Create a shortcut to each .exe
    shell = Dispatch('WScript.Shell')
    for i in range(2):
        shortcut = shell.CreateShortCut(ShortcutDestinationPaths[i]) # Location where the Shortcut is created
        shortcut.Targetpath = ShortcutSourcePaths[i] # What the Shortcut links too
        shortcut.save()

def ReleaseBuildFinished(Start, End):
    # Calulate how long the ReleaseBuild took, with 2 decimals
    Time = str(round(End - Start, 2))

    print("___________________________________")
    print(f"\nFinished Release in {Time} seconds")
    print(f'\nBuild location: {ScriptPath}\{ReleaseFolder}')
    print("___________________________________")

def CreateRelease():
    Start = time.time()

    SetupRelease()

    BuildProjects()

    MoveBuildResults()

    CreateShortcuts()

    End = time.time()

    ReleaseBuildFinished(Start, End)

# Write out header
print("___________________________________")
print("\nReleaseBuilder v1.0.0")
print("___________________________________")

# Set all variables
ScriptPath = os.getcwd()

ReleaseName = input("\nName of Release: ")
ReleaseFolder = f'Releases\{ReleaseName}'

ProjectsPath = [f'{ScriptPath}\SpeedrunNotes\SpeedrunNotes.csproj', f'{ScriptPath}\SpeedrunNotesEditor\SpeedrunNotesEditor.csproj']
BuildSourcePaths = [f'{ScriptPath}/SpeedrunNotes/bin/Release/net7.0-windows10.0.19041.0/win10-x64', f'{ScriptPath}/SpeedrunNotesEditor/bin/Release/net7.0-windows10.0.19041.0/win10-x64']
BuildDestinationPaths = [f'{ReleaseFolder}/SpeedrunNotes', f'{ReleaseFolder}/SpeedrunNotesEditor']

ShortcutSourcePaths = [f'{BuildSourcePaths[0]}\SpeedrunNotes.exe', f'{BuildSourcePaths[1]}\SpeedrunNotesEditor.exe']
ShortcutDestinationPaths = [f'{ReleaseFolder}\SpeedrunNotes.lnk', f'{ReleaseFolder}\SpeedrunNotesEditor.lnk']

MSBuild = r'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'

Arg1 = '/p:Configuration=Release'

# Write out more stuff
print("\nProjects to build: \n  -SpeedrunNotes \n  -SpeedrunNotesEditor")
print(f"\nBuild destination: {ScriptPath}\{ReleaseFolder}")
AAA = input("\nPress Enter to start building the Release")
print("___________________________________")

CreateRelease()

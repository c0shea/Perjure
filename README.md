# Perjure

Perjure is a small, light-weight console application that allows you to purge folders after the number of days you define. Perjure works well with job schedulers that run the program daily.

## Installation

Perjure requires no installation and can be deployed via ```xcopy```. Simply unzip the release, modify the ```Configuration.json``` file to fit your needs, and run ```Perjure.exe``` from the command line.

## Usage

### Step 1

Configure the settings file. The ```Configuration.json``` file defines all of the purge rules that will be executed.

    [
      {
        "DirectoryPath": "D:\\DirectoryWhoseFilesToPurge",
        "MatchPattern": "",
        "DaysToPurgeAfter": 30,
        "MinimumFilesToKeep": 1,
        "IncludeSubfolders": false,
        "IncludeHiddenFiles": false,
        "DeleteEmptySubdirectories": true,
        "TimeComparison": "LastWrite"
      }
    ]

The MatchPattern property is a C# regular expression. This allows for more advanced file matching than just the built-in wildcard operators. An empty match pattern is the same as using a wildcard (```*```).

### Step 2

Run the program. There is only one command line parameter, which is the fully qualified path to the ```Configuration.json``` file. If this parameter isn't specified, it defaults to the current executing location.

Specifying a different location for the settings file:

    Perjure.exe "D:\Configuration.json"

Using the default settings file:

    Perjure.exe

> Note: The program does not prompt before deleting files. You should be sure that the directories specified in the settings file are what you *actually* want the program to purge before running the program.

## Exit Codes
The program will return different exit codes so that appropriate error handling can be added to the calling application. This allows enterprise job schedulers, for example, to fail the job if Perjure returns an exit code other than ```0```.

* **0 - Success:** The program completed normally
* **1 - Invalid Configuration:** The configuration file is not formatted correctly or the file could not be found
* **2 - Directory Not Found:** A directory in the configuration file was not found and could not be purged
* **4 - File Not Deleted:** A file was not deleted, which can be caused by any number of reasons. The file may be marked as read-only, the user account running Perjure does not have access to delete the file, or the file existed when the list of files to delete was constructed but was deleted by another user or process before Perjure could delete the file.

Since the exit codes are a C# Flags Enum, additional exit codes can be returned which are a combination of those above. For example, ```6``` could be returned, which means that both a directory was not found and a file was not deleted.

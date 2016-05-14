# Perjure

Perjure is a small, light-weight console application that allows you to purge folders after the number of days you define. Perjure works well with job schedulers that run the program daily.

## Installation

Perjure requires no installation and can be deployed via ```xcopy```. Simply unzip the release, modify the ```Settings.json``` file to fit your needs, and run ```Perjure.exe``` from the command line.

## Usage

### Step 1

Configure the settings file. The ```Settings.json``` file defines all of the purge rules that will be executed.

    [
      {
        "DirectoryPath": "D:\\DirectoryWhoseFilesToPurge",
        "MatchPattern": "",
        "DaysToPurgeAfter": 30,
        "IncludeSubfolders": false,
        "IncludeHiddenFiles": false
      }
    ]

The MatchPattern property is a C# regular expression. This allows for more advanced file matching than just the built-in wildcard operators. An empty match pattern is the same as using a wildcard (```*```).

### Step 2

Run the program. There is only one command line parameter, which is the fully qualified path to the ```Settings.json``` file. If this parameter isn't specified, it defaults to the current executing location.

Specifying a different location for the settings file:

    Perjure.exe "D:\Settings.json"

Using the default settings file:

    Perjure.exe

> Note: The program does not prompt before deleting files. You should be sure that the directories specified in the settings file are what you *actually* want the program to purge before running the program.

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request
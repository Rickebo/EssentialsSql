# EssentialsSql

EssentialsSql is a windows tool used to store the Minecraft Spigot plugin
[Essentials](https://github.com/EssentialsX/Essentials) generated userdata files in a SQL database, with some properties
stored in separate columns, such as Money. The tool was created to enable indexed lookups of what players have the most
money on an Essentials server, greatly accelerating the performance of the `/baltop` command.

The tool uses [SQLFS](https://github.com/Rickebo/SQLFS) and Dokan to mount a custom file system in usermode to the
Essentials userdata directory. Reads and writes to the directory are then handled by the tool, which in turn forwards them to
the database.

## Prerequisites

- Windows
- [Dokan](https://github.com/dokan-dev/dokany) (preferably version 1.4.0)
- [.NET 5](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

## Usage

1. Install all prerequisites.
2. Clone this repository and build the tool using .NET 5.
3. Create and configure a `settings.json`
   [(example)](https://github.com/Rickebo/EssentialsSql/blob/master/EssentialsSql/settings.json) file in the tools
   working directory.
4. Start the tool.

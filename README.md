# scry

Command line utility for querying Magic the Gathering collections. Joins search results from Scryfall with CSV inventory file.

## Usage

1. Create a CSV file of your inventory, with these columns in this order, and no header row:
    * Count
    * Name
    * Set
2. Open `appsettings.json` and set `InventoryPath` to the location of your inventory CSV file.
3. Add the app directory to the `PATH` environment variable, so it can be used from the command line easily.
4. Run `scry -q="<your-query>"`, where `<your-query>` is any query using [Scryfall query syntax](https://scryfall.com/docs/syntax).

### Arguments

* `-q` or `--query` - a Scryfall query
* `-i` or `--inventory` - the path to an inventory CSV file. (Setting this permanently in appsettings.json is more convenient.)

## Debugging

Arguments to `dotnet run` after `--` are passed to the application being run.

`dotnet run -- -q="<query>"`

## Issues

* No support for differentiating alternate art or foil versions in inventory
* Joining set information to Scryfall would allow more readable output

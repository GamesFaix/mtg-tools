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

### Example output

```
scry -q="mana"
Loading set information from Scryfall...
  Found 698 results
Searching Scryfall for "mana"...
  Found 54 results
Loading inventory from C:\Users\NBolas\Documents\inventory.csv...
  Found 7088 editions, and 15460 total cards.
Joining search results with inventory...
  Found 11 distinct cards, 12 editions, and 42 total cards.

Manabarbs                      Enchantment                   3R  (x3)
   (x1) Magic 2012 (M12)
   (x2) Fourth Edition (4ED)

Mana Cylix                     Artifact                       1  (x1)
   (x1) Planeshift (PLS)

Mana Geode                     Artifact                       3 (x19)
  (x19) War of the Spark (WAR)

Mana Geyser                    Sorcery                      3RR  (x2)
   (x2) Fifth Dawn (5DN)

Manakin                        Artifact Creature - Construct      2  (x1)
   (x1) Iconic Masters (IMA)

.
.
.
```

## Debugging

Arguments to `dotnet run` after `--` are passed to the application being run.

`dotnet run -- -q="<query>"`

## Issues

* No support for differentiating alternate art or foil versions in inventory

# Archivist

Archivist is a CLI for Magic: the Gathering inventory management.

## Usage

### Create a workspace

First set a workspace directory.
This is the folder where all your Archivist files will go.

```powershell
archivist workspace C:\Users\C.Nalar\Documents\Archivist
```

You can check the current workspace with:

```powershell
archivist workspace
Workspace is currently set to C:\Users\C.Nalar\Documents\Archivist.
```

Add files for transactions to subfolders of the `transactions` folder.
Currenlty the only format supported is `.txt` files from DragonShield's mobile app.

### Calculate your current inventory

```powershell
archivist createInventory
```

This will sum all transactions to generate these files:

```
Workspace
|- inventory
   |- current
      |- cards.csv
      |- manifest.json
```

* `cards.csv` contains a list of all the cards currently in your inventory.
* `manifest.json` contains some metadata about how the csv was generated.


## Workspace structure

```
C:\Users\C.Nalar\Documents\Archivist
|- inventory
   | current
     | manifest.json
     | cards.csv
|- transactions
   |- 01-m21-boosters
      |- cards.csv
   |- 02-bulk-lot
      |- cards1.csv
      |- cards2.csv
      |- ...
   |- 03-trade-with-jace
      |- manifest.json
      |- incoming.csv
      |- outgoing.csv
   |- ...
```

### Transactions

A transaction represents a set of cards acquired, lost, or traded together. Each transaction's files are saved in a subfolder of the `transactions/` folder. Transations are numbered in the order they are created.

The current state of the inventory can be calculated by "adding" each transaction together.

Each transaction folder has one `manifest.json` file and one or more `.csv` files.

### `manifest.json`

```json
// 01-m21-boosters/manifest.json
{
    "id": 1,
    "title": "M21 boosters", // required
    "date": "2020-01-02", // required
    "notes": "2x", // optional
    "cost": 8.00, // optional
    "add": [ "cards" ] // optional, but either 'add' or 'remove' required
}
```

```fsharp
type Transaction = {
    Id : int
    Title : string
    Date : DateTime
    Notes : string option
    Cost : Decimal option
    Add : string list
    Remove : string list
}
```

## Card CSV structure

|Count|Name|Set|Foil|No|Lang
|-|-|-|-|-|-|
|2|Lightning bolt|4ED
|1|Gravedigger|7ED|true
|2|Island|STX| |253
|1|Island|STX| |254
|1|Island|STX| |254|Spanish

```fsharp
type Card = {
    Count: int          // required
    Name: string        // required
    Set: string         // required
    Foil: bool          // false if missing
    No: int option      // Only needed to disambiguate alternate art
    Lang: string option // English if None
}
```

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

Each transaction folder has one or more data files and optionally a manifest file. A manifest can contain metadata about the transaction, as well as specify if some data files should be subtracted from the inventory instead of added. By default, each data file is assumed to be adding cards, since typically collections get bigger over time.

### `manifest.json`

```json
// 2020-01-02-trade-with-jace/manifest.json
{
    "id": 1,
    "date": "2020-01-02", // if blank, date in folder name will be used
    "title": "Trade with Jace", // if blank, part of folder name after date will be used
    "notes": "2x", // optional
    "cost": 8.00, // optional
    "addFiles": [ "cards" ], // optional
    "removeFiles": [ "] // optional
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

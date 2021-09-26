module GamesFaix.MtgTools.Archivist.Model

open System

// DragonShield exports don't have headers, so the property order matters
type DragonShieldCsvCard = {
    Count : int
    Name : string
    Set : string
    Condition : string
    Price : decimal
    Version : string
    Language : string
    Date : DateTime
}

type InventoryCsvCard = {
    Count : int
    Name : string
    Set : string
    Version : string
    Language : string
}

type Card = {
    Name : string
    Set : string
    Version : string
    Language : string
}

type CardCount = int * Card

module Card =
    
    let fromDragonShieldCsv (c: DragonShieldCsvCard) : CardCount =
        c.Count,
        {
            Name = c.Name.Trim()
            Set = c.Set.Trim()
            Version = c.Version.Trim()
            Language = c.Language.Trim()
        }
    
    let toInventoryCsv ((ct, c): CardCount) : InventoryCsvCard =
        {
            Count = ct
            Name = c.Name
            Set = c.Set
            Version = c.Version
            Language = c.Language
        }

type TransactionInfo = {
    Title : string
    Date : DateTime
    Price : decimal option
    Notes : string option
}

// Structure of manifest.json file
type TransactionManifest = {
    Info : TransactionInfo
    AddFiles : string list option
    SubtractFiles : string list option
}

// Hydrated manifest file
type TransactionDetails = {
    Info : TransactionInfo
    Add : CardCount list
    Subtract : CardCount list
}

type Inventory = {
    Transactions : TransactionInfo list
    Cards : CardCount list
}

type InventoryManifest = {
    Transactions : TransactionInfo list
}

module Inventory =
    let empty = { Transactions = []; Cards = [] }

    let manifest (inv: Inventory) : InventoryManifest =
        {
            Transactions = inv.Transactions
        }

type JobResult = Async<Result<unit, string>>
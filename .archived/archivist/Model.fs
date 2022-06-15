module GamesFaix.MtgTools.Archivist.Model

open System
open GamesFaix.MtgTools.Shared.Inventory

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

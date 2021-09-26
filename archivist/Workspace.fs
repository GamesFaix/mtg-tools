module GamesFaix.MtgTools.Archivist.Workspace

open System.IO
open System

type InventoryDirectory = {
    Path : string
}
module InventoryDirectory =
    let create (rootDir: string) : InventoryDirectory =
        {
            Path = Path.Combine(rootDir, "inventory")
        }

type TransactionDirectory = {
    Path : string
    Manifest : string
    GetCardFiles : unit -> string list
    GetCardFile : string -> string
}
module TransactionDirectory =

    let create (rootDir: string) (name: string) : TransactionDirectory =
        let path = Path.Combine(rootDir, name)
        {
            Path = path
            Manifest = Path.Combine(path, "manifest.json")
            GetCardFiles = (fun () ->
                Directory.EnumerateFiles(path)
                |> Seq.filter (fun f -> not <| f.EndsWith(".json"))
                |> Seq.toList
            )
            GetCardFile = (fun name -> Path.Combine(path, name))
        }

type TransactionsDirectory = {
    Path : string
    GetTransactionDirectories : unit -> string list
    GetTransactionDirectory : string -> TransactionDirectory
}
module TransactionsDirectory =
    let create (rootDir: string) : TransactionsDirectory =
        let path = Path.Combine(rootDir, "transactions")
        {
            Path = path
            GetTransactionDirectories = (fun () -> Directory.GetDirectories path |> Seq.toList)
            GetTransactionDirectory = TransactionDirectory.create rootDir
        }

type WorkspaceDirectory = {
    Path : string
    Inventory : InventoryDirectory
    Transactions : TransactionsDirectory
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        let rootDir =
            rootDir
            |> Environment.ExpandEnvironmentVariables
            |> Path.GetFullPath

        {
            Path = rootDir
            Inventory = InventoryDirectory.create rootDir
            Transactions = TransactionsDirectory.create rootDir
        }

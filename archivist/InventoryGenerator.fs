module GamesFaix.MtgTools.Archivist.InventoryGenerator

open System
open Model
open Context

let add (a: CardCount list) (b: CardCount list) : CardCount list =
    a |> List.append b
    |> List.groupBy snd
    |> List.map (fun (card, xs) -> (xs |> List.sumBy fst, card))
    |> List.filter (fun (ct, _) -> ct <> 0) // Keep any negative counts for validation

let subtract (toSubtract: CardCount list) (given: CardCount list) : CardCount list =
    toSubtract
    |> List.map (fun (ct, card) -> (0-ct, card)) // Invert then add
    |> add given

let private apply (inv: Inventory) (tran: TransactionDetails) : Inventory =
    {
        Cards = inv.Cards |> add tran.Add |> subtract tran.Subtract
        Transactions = tran.Info :: inv.Transactions
    }

let private loadAndApply (inv: Inventory) (transactionName: string) (ctx: WorkspaceContext) : Result<Inventory, string> Async =
    async {
        let dir = ctx.Workspace.Transactions.GetTransactionDirectory transactionName
        ctx.Log.Information $"Loading transaction {transactionName}..."
        let! result = TransactionLoader.loadTransactionDetails dir ctx.Log
        let result = result |> Result.map (apply inv)
        match result with
        | Ok inv -> ctx.Log.Information $"{inv.Cards.Length} unique cards, {inv.Cards |> Seq.sumBy fst} total cards in inventory."
        | _ -> ()
        return result
    }

let private computeInventory (ctx: WorkspaceContext) : Result<Inventory, string> Async =
    async {
        let dir = ctx.Workspace.Transactions
        ctx.Log.Information "Starting with empty inventory..."
        let mutable result = Ok Inventory.empty
        for t in dir.GetTransactionDirectories() do
            match result with
            | Ok inv ->
                let! r = loadAndApply inv t ctx
                result <- r
            | _ -> ()

        return result
    }

let private saveManifest (inv: Inventory) (ctx: WorkspaceContext) : unit Async =
    let manifest = Inventory.manifest inv
    let path = ctx.Workspace.Inventory.Current.Manifest
    FileSystem.saveToJson manifest path

let private saveCards (inv: Inventory) (ctx: WorkspaceContext) : unit Async =
    let path = ctx.Workspace.Inventory.Current.Cards
    Csv.saveCardFile path inv.Cards

let generate (ctx: WorkspaceContext) : Result<unit, string> Async =
    async {
        match! computeInventory ctx with
        | Ok inv ->
            match Auditor.validate inv with
            | Ok () ->
                do! saveManifest inv ctx
                do! saveCards inv ctx
                return Ok ()
            | Error issues ->
                return Error (String.Join('\n', issues))
        | Error err -> return Error err
    }
module GamesFaix.MtgTools.Archivist.TransactionSummer

open Model
open Context

let add (a: CardCount list) (b: CardCount list) : CardCount list =
    a |> List.append b
    |> List.groupBy snd
    |> List.map (fun (card, xs) -> (xs |> List.sumBy fst, card))

let subtract (given: CardCount list) (toSubtract: CardCount list) : CardCount list =
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
        let! result = TransactionLoader.loadTransactionDetails dir ctx.Log
        return result |> Result.map (apply inv)
    }

let generateInventory (ctx: WorkspaceContext) : Result<Inventory, string> Async =
    async {
        let dir = ctx.Workspace.Transactions
        let mutable result = Ok Inventory.empty
        for t in dir.GetTransactionDirectories() do
            match result with
            | Ok inv ->
                let! r = loadAndApply inv t ctx
                result <- r
            | _ -> ()

        return result
    }
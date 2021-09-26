module GamesFaix.MtgTools.Archivist.TransactionSummer

open Model

let add (a: CardCount list) (b: CardCount list) : CardCount list =

    failwith "not implemented"

let subtract (given: CardCount list) (toSubtract: CardCount list) : CardCount list =

    failwith "not implemented"

let apply (inv: Inventory) (tran: TransactionDetails) : Inventory =
    {
        Cards = inv.Cards |> add tran.Add |> subtract tran.Subtract
        Transactions = tran.Info :: inv.Transactions
    }
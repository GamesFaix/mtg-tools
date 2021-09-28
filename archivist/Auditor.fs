module GamesFaix.MtgTools.Archivist.Auditor

open Model

let validate (inv: Inventory) : Result<unit, string list> =
    let issues = ResizeArray()

    // Negative counts
    inv.Cards
    |> Seq.filter (fun (ct, card) -> ct < 0)
    |> Seq.map (fun (ct, card) -> $"{ct} {card.Name} ({card.Set})")
    |> issues.AddRange

    // Zero counts
    inv.Cards 
    |> Seq.filter (fun (ct, card) -> ct = 0)
    |> Seq.map (fun (ct, card) -> $"{ct} {card.Name} ({card.Set})")
    |> issues.AddRange

    match issues.Count with
    | 0 -> Ok ()
    | _ -> Error (issues |> Seq.toList)
    
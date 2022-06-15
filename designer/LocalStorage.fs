module GamesFaix.MtgTools.Designer.LocalStorage

open GamesFaix.MtgTools.Designer.Model
open GamesFaix.MtgTools.Designer.Context
open GamesFaix.MtgTools.Shared
open System.Drawing

let saveCardImage (card: CardInfo) (bytes: byte[]) (ctx: UserContext) = async {
    let path = ctx.Workspace.Set(card.Set).CardImage(card.Name)
    return! FileSystem.saveFileBytes bytes path
}

let loadCardImage (card: CardInfo) (ctx: UserContext) = async {
    let path = ctx.Workspace.Set(card.Set).CardImage(card.Name)
    return Bitmap.FromFile path
}

let saveSetDetails (setAbbrev: string) (cards: CardDetails list) (ctx: UserContext) = async {
    let path = ctx.Workspace.Set(setAbbrev).JsonDetails
    do! FileSystem.saveToJson cards path
}

let loadSetDetails (setAbbrev: string) (ctx: UserContext) = async {
    let path = ctx.Workspace.Set(setAbbrev).JsonDetails
    let! details = FileSystem.loadFromJson<CardDetails list> path
    return details |> Option.defaultValue []
}


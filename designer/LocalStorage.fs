module GamesFaix.MtgTools.Designer.LocalStorage

open GamesFaix.MtgTools.Designer.Model
open GamesFaix.MtgTools.Designer.Context
open GamesFaix.MtgTools.Shared
open System.Drawing

let private setDir (setAbbrev: string) (ctx: UserContext) =
    ctx.Workspace.Path /- setAbbrev

let private escapeCardNameForPath (cardName: string) =
    cardName.Replace(" ", "-").Replace("?", "-")

// -----

let cardImageRelativePath (card: CardInfo) =
    card.Set /- "cards" /- (escapeCardNameForPath card.Name) + ".jpg"

let saveCardImage (card: CardInfo) (bytes: byte[]) (ctx: UserContext) = async {
    let path = ctx.Workspace.Path /- cardImageRelativePath card
    return! FileSystem.saveFileBytes bytes path
}

let loadCardImage (card: CardInfo) (ctx: UserContext) = async {
    let path = ctx.Workspace.Path /- cardImageRelativePath card
    return Bitmap.FromFile path :?> Bitmap
}

let clearCardImages (setAbbrev: string) (ctx: UserContext) = async {
    let path = setDir setAbbrev ctx /- "cards"
    do! FileSystem.deleteFilesInFolderMatching path (fun f -> f.EndsWith ".jpg")
}

// -----

let saveMpcRender (card: CardInfo) (img: Bitmap) (ctx: UserContext) = async {
    let path = 
        (setDir card.Set ctx) 
        /- "mpc" 
        /- (escapeCardNameForPath card.Name) + ".jpg"
    img.Save path
}

// -----

let saveSetDetails (setAbbrev: string) (cards: CardDetails list) (ctx: UserContext) = async {
    let path = (setDir setAbbrev ctx) /- "details.json"
    do! FileSystem.saveToJson cards path
}

let loadSetDetails (setAbbrev: string) (ctx: UserContext) = async {
    let path = (setDir setAbbrev ctx) /- "details.json"
    let! details = FileSystem.loadFromJson<CardDetails list> path
    return details |> Option.defaultValue []
}

// -----

let loadSetCenterFixes (setAbbrev: string) (ctx: UserContext) = async {
    let path = (setDir setAbbrev ctx) /- "center-fixes.json"
    let! details = FileSystem.loadFromJson<string list> path
    return details |> Option.defaultValue []
}

// -----

let saveSetLayout (html: string) (setAbbrev: string) (ctx: UserContext) = async {
    let path = (setDir setAbbrev ctx) /- "layout.html"
    do! FileSystem.saveFileText html path
}

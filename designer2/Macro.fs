module GamesFaix.MtgTools.Designer.Macro

open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Model

let login (ctx: Context) : unit Async =
    async {
        do! MtgDesign.Auth.login ctx
        return ()
    }

module Card =

    let copy (ctx: Context) (oldSetAbbrev: string) (cardName: string) (newSetAbbrev: string) : unit Async =
        async {
            ctx.Log $"Copying card {cardName} from {oldSetAbbrev} to {newSetAbbrev}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx oldSetAbbrev
            let card = cardInfos |> Seq.find (fun c -> c.Name = cardName)
            let! details = MtgDesign.Reader.getCardDetails ctx card
            let centerFixes = FileSystem.loadCenterFixes ctx.RootDir oldSetAbbrev
            let details = CardProcessor.processCard centerFixes details
            let details = { details with Set = newSetAbbrev }
            let! _ = MtgDesign.Writer.saveCards ctx MtgDesign.Writer.SaveMode.Create [details]
            ctx.Log "Done."
            return()
        }

    let delete (ctx: Context) (setAbbrev : string) (cardName: string) : unit Async =
        async {
            ctx.Log $"Deleting card {setAbbrev} - {cardName}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let card = cardInfos |> Seq.find (fun c -> c.Name = cardName)
            let! _ = MtgDesign.Writer.deleteCard ctx card
            ctx.Log "Done."
            return ()
        }

module Set =
    let private loadCards ctx setAbbrev =
        async {
            let! cards = MtgDesign.Reader.getSetCardDetails ctx setAbbrev
            let centerFixes = FileSystem.loadCenterFixes ctx.RootDir setAbbrev
            return CardProcessor.processCards ctx.Logger centerFixes cards
        }

    let audit (ctx: Context) (setAbbrev: string) : unit Async =
        async {
            ctx.Log $"Auditing set {setAbbrev}..."
            let! cards = loadCards ctx setAbbrev
            Auditor.findIssues cards
            |> Auditor.logIssues ctx.Logger
            printfn "Done."
            return ()
        }

    let autonumber (ctx: Context) (setAbbrev: string) : unit Async =
        async {
            ctx.Log $"Autonumnbering set {setAbbrev}..."
            let! cards = loadCards ctx setAbbrev
            let! _ = MtgDesign.Writer.saveCards ctx MtgDesign.Writer.SaveMode.Edit cards
            ctx.Log "Done."
            return ()
        }

    let copy (ctx: Context) (oldAbbrev : string) (newAbbrev: string) : unit Async =
        async {
            ctx.Log $"Copying set {oldAbbrev} to {newAbbrev}..."
            let! cards = loadCards ctx oldAbbrev
            let cards = cards |> List.map (fun c -> { c with Set = newAbbrev })
            let! _ = MtgDesign.Writer.saveCards ctx MtgDesign.Writer.SaveMode.Create cards
            ctx.Log "Done."
            return ()
        }

    let createHtmlLayout (ctx: Context) (setAbbrev: string) : unit Async =
        async {
            ctx.Log $"Creating HTML layout for set {setAbbrev}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let html = Layout.createHtmlLayout cardInfos
            let! _ = FileSystem.saveHtmlLayout ctx.RootDir html setAbbrev
            ctx.Log "Done."
            return ()
        }

    let delete (ctx: Context) (setAbbrev : string) : unit Async =
        async {
            ctx.Log $"Deleting set {setAbbrev}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let! _ = MtgDesign.Writer.deleteCards ctx cardInfos
            ctx.Log "Done."
            return ()
        }

    let downloadImages (ctx: Context) (setAbbrev: string) : unit Async =
        let downloadImage (card: CardInfo) =
            async {
                ctx.Log $"Downloading image for card {card.Name}..."
                let! bytes = MtgDesign.Reader.getCardImage ctx card
                let! _ = FileSystem.saveCardImage ctx.RootDir bytes card
                return ()
            }

        async {
            ctx.Log $"Downloading images for set {setAbbrev}..."
            FileSystem.deleteSetFolder ctx.RootDir setAbbrev
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let! _ =
                cardInfos
                |> List.map downloadImage
                |> Async.Parallel
            ctx.Log "Done."
            return ()
        }

    let rename (ctx: Context) (oldAbbrev : string) (newAbbrev: string) : unit Async =
        async {
            ctx.Log $"Renaming set {oldAbbrev} to {newAbbrev}..."
            let! cards = loadCards ctx oldAbbrev
            let cards = cards |> List.map (fun c -> { c with Set = newAbbrev })
            let! _ = MtgDesign.Writer.saveCards ctx MtgDesign.Writer.SaveMode.Edit cards
            ctx.Log "Done."
            return ()
        }

module GamesFaix.MtgTools.Designer.Macro

open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Model

module Set =
    let private loadCards ctx setAbbrev =
        async {
            let! cards = MtgDesign.Reader.getSetCardDetails ctx setAbbrev
            let centerFixes = FileSystem.loadCenterFixes ctx.Workspace setAbbrev
            return CardProcessor.processCards ctx.Logger centerFixes cards
        }

    let createHtmlLayout (ctx: Context) (setAbbrev: string) : unit Async =
        async {
            ctx.Log $"Creating HTML layout for set {setAbbrev}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let html = Layout.createHtmlLayout cardInfos
            let! _ = FileSystem.saveHtmlLayout ctx.Workspace html setAbbrev
            ctx.Log "Done."
            return ()
        }

    let downloadImages (ctx: Context) (setAbbrev: string) : unit Async =
        let downloadImage (card: CardInfo) =
            async {
                ctx.Log $"Downloading image for card {card.Name}..."
                let! bytes = MtgDesign.Reader.getCardImage ctx card
                let! _ = FileSystem.saveCardImage ctx.Workspace bytes card
                return ()
            }

        async {
            ctx.Log $"Downloading images for set {setAbbrev}..."
            FileSystem.deleteSetFolder ctx.Workspace setAbbrev
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx setAbbrev
            let! _ =
                cardInfos
                |> List.map downloadImage
                |> Async.Parallel
            ctx.Log "Done."
            return ()
        }


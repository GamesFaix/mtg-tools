module GamesFaix.MtgTools.Designer.Macro

open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Model

let cloneSet (ctx: Context) (oldAbbrev : string) (newAbbrev: string) : unit Async =
    async {
        ctx.Log $"Cloning {oldAbbrev} to {newAbbrev}..."
        let! cardDetails = MtgDesign.Reader.getSetCardDetails ctx oldAbbrev
        let centerFixes = FileSystem.loadCenterFixes ctx.RootDir oldAbbrev
        let processed =
            CardProcessor.processCards ctx.Logger centerFixes cardDetails
            |> List.map (fun c -> { c with Set = newAbbrev })
        let! _ = MtgDesign.Writer.saveCards ctx MtgDesign.Writer.SaveMode.Create processed
        ctx.Log "Done."
        return ()
    }

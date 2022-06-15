module GamesFaix.MtgTools.Designer.MpcRender

open System.Drawing
open Model
open Context
open Workspace.SetDirectory
open GamesFaix.MtgTools.Shared.Utils

// See https://www.reddit.com/r/mpcproxies/comments/e9q1z7/complete_guide_to_image_sizing_for_mpc_or_other/
let private mpcCardSize = Size(816, 1100)

let private padImage (sourcePath: string) (targetPath: string) (borderColor: Color) = async {
    let source = new Bitmap(sourcePath)
    let target = new Bitmap(mpcCardSize.Width, mpcCardSize.Height)

    let offset = Point(
        (target.Width - source.Width) / 2,
        (target.Height - source.Height) / 2
    )
    let brush = new SolidBrush(borderColor)
    let background = Rectangle(0, 0, mpcCardSize.Width, mpcCardSize.Height)
    
    let g = Graphics.FromImage target
    g.FillRectangle(brush, background)
    g.DrawImage(source, offset)
    target.Save targetPath

    return ()
}

let private silver = Color.FromArgb(159, 159, 159)
let private getBorderColor (card: CardDetails) =
    match card.Border with
    | "black" -> Color.Black
    | "white" -> Color.White
    | "silver" -> silver
    | "gold" -> Color.Gold // TODO: Correct gold border color
    | _  -> failwith $"Invalid border color: {card.Border}"

let padForMpc (cards: CardDetails list) (ctx: UserContext) = async {
    for c in cards do
        ctx.Log.Information $"\tRendering {c.Name}..."
        let setDirectory = ctx.Workspace.Set c.Set
        let sourceFile = getCardFileName c.Name
        let targetFile = getMpcCardFileName c.Name
        let sourcePath = setDirectory.Path /- sourceFile
        let targetPath = setDirectory.Path /- targetFile
        let borderColor = getBorderColor c
        do! padImage sourcePath targetPath borderColor
}
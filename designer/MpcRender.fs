module GamesFaix.MtgTools.Designer.MpcRender

open System.Drawing
open Model
open Context
open Workspace.SetDirectory
open GamesFaix.MtgTools.Shared.Utils

// See https://www.reddit.com/r/mpcproxies/comments/e9q1z7/complete_guide_to_image_sizing_for_mpc_or_other/
let private mpcCardSize = Size(816, 1100)
let private borderThickness = 32

let private pad (source: Bitmap) (borderColor: Color) = async {
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
    return target
}

let private eraseCorners (source: Bitmap) = async {
    let copy = new Bitmap(source)
    let g = Graphics.FromImage(copy)
    g.DrawImage(source, Point(0,0))
    
    let range = seq {
        for x in 0..borderThickness do
            for y in 0..borderThickness do
                yield x, y
            for y in (source.Height-borderThickness-1)..(source.Height-1) do
                yield x, y
        for x in (source.Width-borderThickness-1)..(source.Width-1) do
            for y in 0..borderThickness do
                yield x, y
            for y in (source.Height-borderThickness-1)..(source.Height-1) do
                yield x, y
    }

    for x, y in range do
        let px = copy.GetPixel(x, y)
        let threshold = 25uy
        let isCloseToWhite (c:Color) =
            c.R > threshold &&
            c.G > threshold &&
            c.B > threshold
        if isCloseToWhite px then
            copy.SetPixel(x, y, Color.Transparent)
        //else ()
    
    return copy
}

let private silver = Color.FromArgb(159, 159, 159)
let private getBorderColor (card: CardDetails) =
    match card.Border with
    | "black" -> Color.Black
    | "white" -> Color.White
    | "silver" -> silver
    | "gold" -> Color.Gold // TODO: Correct gold border color
    | _  -> failwith $"Invalid border color: {card.Border}"

let renderForMpc (cards: CardDetails list) (ctx: UserContext) = async {
    for c in cards do
        ctx.Log.Information $"\tRendering {c.Name}..."
        let setDirectory = ctx.Workspace.Set c.Set
        let sourceFile = getCardFileName c.Name
        let targetFile = getMpcCardFileName c.Name
        let sourcePath = setDirectory.Path /- sourceFile
        let targetPath = setDirectory.Path /- targetFile
        let borderColor = getBorderColor c
        use source = new Bitmap(sourcePath)
        use! cornersRemoved = eraseCorners source
        use! padded = pad cornersRemoved borderColor
        padded.Save targetPath
}
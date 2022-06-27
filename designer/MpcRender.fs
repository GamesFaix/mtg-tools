module GamesFaix.MtgTools.Designer.MpcRender

open System.Drawing
open Model
open Context
open Workspace.SetDirectory
open GamesFaix.MtgTools.Shared.Utils

// See https://www.reddit.com/r/mpcproxies/comments/e9q1z7/complete_guide_to_image_sizing_for_mpc_or_other/
let private mpcCardSize = Size(802, 1097)
let private borderThickness = 32

let private drawBlackBorderPad (g: Graphics) : unit =
    use brush = new SolidBrush(Color.Black)
    let background = Rectangle(0, 0, mpcCardSize.Width, mpcCardSize.Height)
    g.FillRectangle(brush, background)

let private drawSilverBorderPad (g: Graphics) : unit =
    use topBrush = new SolidBrush(Color.FromArgb(159, 159, 159))
    use bottomBrush = new SolidBrush(Color.Black)
    
    let bottomHeight = 320

    let top = Rectangle(0, 0, mpcCardSize.Width, mpcCardSize.Height-bottomHeight)
    let bottom = Rectangle(0, mpcCardSize.Height-bottomHeight, mpcCardSize.Width, bottomHeight)

    g.FillRectangle(topBrush, top)
    g.FillRectangle(bottomBrush, bottom)

let private getDrawBorderPadStrategy (card: CardDetails) =
    match card.Border with
    | "black" -> drawBlackBorderPad
    | "silver" -> drawSilverBorderPad
    | "white" | "gold" -> failwith "Not implemented"
    | _  -> failwith $"Invalid border color: {card.Border}"

let private pad (source: Bitmap) (drawBorderPad: Graphics -> unit) = async {
    let target = new Bitmap(mpcCardSize.Width, mpcCardSize.Height)

    let offset = 
        Point(
            (target.Width - source.Width) / 2,
            (target.Height - source.Height) / 2
        )
    
    let g = Graphics.FromImage target
    drawBorderPad g
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

let renderForMpc (cards: CardDetails list) (ctx: UserContext) = async {
    for c in cards do
        ctx.Log.Information $"\tRendering {c.Name}..."
        let setDirectory = ctx.Workspace.Set c.Set
        
        let sourcePath = setDirectory.Path /- (getCardFileName c.Name)
        use source = new Bitmap(sourcePath)

        use! cornersRemoved = eraseCorners source
        
        let drawBorderPad = getDrawBorderPadStrategy c
        use! padded = pad cornersRemoved drawBorderPad
        
        let targetPath = setDirectory.Path /- (getMpcCardFileName c.Name)
        padded.Save targetPath
}
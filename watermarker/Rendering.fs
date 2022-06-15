module Rendering

open Model
open ScryfallApi.Client.Models
open System.Drawing
open System.Drawing.Imaging
open FSharp.Control.Tasks
open System.IO

let maxWidth = 375
let maxHeight = 235
let maxSize = Size(maxWidth, maxHeight)

let private toFloat (size: Size) = SizeF(float32 size.Width, float32 size.Height)

let getColor (c: Card) : WatermarkColor =
    let parse (colors: string seq) =
        match colors |> Seq.sort |> Seq.toList with
        | [ "W" ] -> Some White
        | [ "U" ] -> Some Blue
        | [ "B" ] -> Some Black
        | [ "R" ] -> Some Red
        | [ "G" ] -> Some Green
        | [ "U"; "W" ] -> Some WhiteBlue
        | [ "B"; "U" ] -> Some BlueBlack
        | [ "B"; "R" ] -> Some BlackRed
        | [ "G"; "R" ] -> Some RedGreen
        | [ "G"; "W" ] -> Some GreenWhite
        | [ "B"; "W" ] -> Some WhiteBlack
        | [ "R"; "U" ] -> Some BlueRed
        | [ "B"; "G" ] -> Some BlackGreen
        | [ "R"; "W" ] -> Some RedWhite
        | [ "G"; "U" ] -> Some GreenBlue
        | _ -> None

    if c.TypeLine.Contains("Land") then 
        match parse c.ColorIdentity with
        | Some x -> x
        | _ ->
            if c.OracleText.Contains("any color") then Gold
            elif c.ColorIdentity = [| |] then LandColorless
            else Gold
    else
        match parse c.Colors with
        | Some x -> x
        | _ ->
            if c.Colors = [| |] then Colorless
            else Gold

let loadBackground (color: WatermarkColor) =
    use bmp = Bitmap.FromFile(FileSystem.backgroundPath color)
    new Bitmap(bmp, maxSize)
    
let private maskImage (source: Bitmap) (mask: Bitmap) =
    let rect = Rectangle(0, 0, mask.Width, mask.Height)
    let source = BitmapHelper.crop source rect

    for y in [0..source.Height-1] do
        for x in [0..source.Width-1] do
            let sourcePx = source.GetPixel(x, y)
            let maskPx = mask.GetPixel(x, y)
            let color = if maskPx.A = 255uy then sourcePx else Color.Transparent
            source.SetPixel(x, y, color)

    source.MakeTransparent(Color.Transparent)

    source
    
let createWatermarkPng (card: Card) = task {
    let color = getColor card
    let path = FileSystem.watermarkPath card.Set color
    let maskPath = FileSystem.maskPath card.Set
    
    let createMask () = task {
        let svgPath = FileSystem.svgPath card.Set
        let mask = SvgHelper.renderAsLargeAsPossibleInContainerWithNoMargin svgPath (maxSize |> toFloat)
        mask.Save maskPath
        return mask
    }

    let getOrCreateMask () = task {
        if File.Exists maskPath then
            return new Bitmap(maskPath)
        else
            return! createMask ()
    }

    let createWatermark () = task {
        use! mask = getOrCreateMask ()
        use background = loadBackground color
        use watermark = maskImage background mask :> Image
        watermark.Save(path, ImageFormat.Png)
    }
    
    //if File.Exists path then
    //    printfn "Found PNG for %s - %s" card.Name path
    //    return ()
    //else 
    printfn "Rendering PNG for %s - %s..." card.Name path
    return! createWatermark ()
}
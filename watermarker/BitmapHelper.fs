module BitmapHelper

open System.Drawing
open System.Drawing.Imaging

type private PixelSpan = int * Color seq

let private getRow (bmp: Bitmap) (i: int) : PixelSpan =
    i, seq { for x in 0..bmp.Width-1 do bmp.GetPixel(x, i) }

let private getRows (bmp: Bitmap) (reverse: bool): PixelSpan seq =
    let order = [0..bmp.Height-1]
    let order = if reverse then order |> List.rev else order
    seq { for y in order do getRow bmp y }

let private getColumn (bmp: Bitmap) (i: int) : PixelSpan =
    i, seq { for y in 0..bmp.Height-1 do bmp.GetPixel(i, y) }

let private getColumns (bmp: Bitmap) (reverse: bool) : PixelSpan seq =
    let order = [0..bmp.Width-1]
    let order = if reverse then order |> List.rev else order
    seq { for x in order do getColumn bmp x }

let private existsAtLeast<'T> (count: int) (filter: 'T -> bool) (source: 'T seq) =
    let mutable matchCount = 0
    use e = source.GetEnumerator()
    while matchCount < count && e.MoveNext() do
        if filter e.Current then
            matchCount <- matchCount + 1
    matchCount >= count

let getBounds (bmp: Bitmap) : Rectangle =
    // Iterate the rows and columns of pixels, and note the first one in each direction that has a non-transparent pixel

    let pixelCount = 1 // Set to 10 for PLIST icon
    let isVisible ((_, pxs): PixelSpan) = pxs |> existsAtLeast pixelCount (fun px -> px.A > 0uy)

    let top =  getRows bmp false |> Seq.tryFind isVisible |> Option.map (fun (y, _) -> y) |> Option.defaultValue 0
    let bottom = getRows bmp true |> Seq.tryFind isVisible |> Option.map (fun (y, _) -> y) |> Option.defaultValue 0
    let left = getColumns bmp false |> Seq.tryFind isVisible |> Option.map (fun (x, _) -> x) |> Option.defaultValue 0
    let right = getColumns bmp true |> Seq.tryFind isVisible |> Option.map (fun (x, _) -> x) |> Option.defaultValue 0

    Rectangle(left, top, right-left+1, bottom-top+1)

let crop (bmp: Bitmap) (rect: Rectangle) =
    bmp.Clone(rect, PixelFormat.Format32bppArgb)

module SvgHelper

open System.Drawing
open Svg
open System

let private getMaxScaleForContainer (containerSize: SizeF) (contentSize: SizeF) : float32 =
    let maxXScale = float32 containerSize.Width / contentSize.Width
    let maxYScale = float32 containerSize.Height / contentSize.Height
    Math.Min(maxXScale, maxYScale)

let private scaleRect (rect: Rectangle) (scale: float32) : Rectangle =
    let left = float32 rect.Left * scale |> int
    let top = float32 rect.Top * scale |> int
    let width = float32 rect.Width * scale |> int
    let height = float32 rect.Height * scale |> int
    Rectangle(left, top, width, height)

let renderAsLargeAsPossibleInContainerWithNoMargin (svgPath: string) (containerSize: SizeF) : Bitmap =
    // Open the SVG
    let svg = SvgDocument.Open(svgPath)
    let dimensions = svg.GetDimensions()
    let svgWidth = dimensions.Width
    let svgHeight = dimensions.Height

    // Render it large to calculate the bounding box
    let preRenderScale = 
        match svgWidth with
        | x when x > 1000f -> 1f
        | x -> 1000f/x

    let rasterWidth = svgWidth * preRenderScale |> int
    let rasterHeight = svgHeight * preRenderScale |> int
    use preRenderBmp = svg.Draw(rasterWidth, rasterHeight)

    // Find the bounds of the content in the BMP, and use that to determine the largest scale the SVG
    // can be rendered at, while the content still fits in the container
    let preRenderBounds = BitmapHelper.getBounds preRenderBmp
    let contentSize = SizeF(float32 preRenderBounds.Width, float32 preRenderBounds.Height)
    let reductionScale = getMaxScaleForContainer containerSize contentSize
    let scale = reductionScale * preRenderScale 

    // Render the SVG to BMP again, at the right scale
    let rasterWidth = scale * svgWidth |> int
    let rasterHeight = scale * svgHeight |> int
    use resizedBmp = svg.Draw(rasterWidth, rasterHeight)

    // Find the scaled bounds of the content, and crop the BMP to remove whitespace
    let bounds = scaleRect preRenderBounds reductionScale
    BitmapHelper.crop resizedBmp bounds

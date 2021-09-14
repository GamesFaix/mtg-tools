module Layouter

open Model
open FSharp.Control.Tasks
open System.Threading.Tasks
open SelectPdf

let private getStyleTag (heightInches: float, widthInches: float) : string =
    sprintf "<style> body { margin: 0; } img { height: %fin; width: %fin; padding-right: 0.15625in; padding-bottom: 0.53125in; }</style>" heightInches widthInches

let private getImageTag (card: CardInfo) : string =
    sprintf "<img src=\"%s\"/>" (FileReaderWriter.getCardFileName card)

let createHtmlLayout (cards : CardInfo list) : string =
    let styleTag = getStyleTag (3.46875, 2.46875)
    let cardTags = cards |> List.map getImageTag

    [
        "<html>"
        "<head>"
        styleTag
        "</head>"
        "<body>"
        cardTags |> String.concat ""
        "</body>"
        "</html>"
    ] |> String.concat "\n"

let convertToPdf (html: string) : byte[] Task =
    task {
        let converter = HtmlToPdf()
        converter.Options.PdfPageSize <- PdfPageSize.Letter
        converter.Options.PdfPageOrientation <- PdfPageOrientation.Landscape
        converter.Options.MarginBottom <- 36 // 72pt/in * 1/2in 
        converter.Options.MarginTop <-    36 // 72pt/in * 1/2in 
        converter.Options.MarginRight <-  18 // 72pt/in * 1/4in 
        converter.Options.MarginLeft <-   18 // 72pt/in * 1/4in 

        let doc = converter.ConvertHtmlString html
        let bytes = doc.Save()
        doc.Close()

        return bytes
    }

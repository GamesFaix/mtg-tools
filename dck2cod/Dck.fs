module GamesFaix.MtgTools.Dck2Cod.Dck

open System
open System.Text.RegularExpressions

type DckTitle = {
    Name: string
    Description: string
}

type DckCard = {
    Name: string
    Id: int
    Count: int
}

type Line =
    | Title of DckTitle
    | Blank
    | Card of DckCard
    | SectionHeader of string

type DckExtension = {
    Name : string
    Cards: DckCard list
}

type DckDeck = {
    Name : string
    Description : string
    Cards : DckCard list
    Extensions : DckExtension list
}

module Line =
    let isBlank (line: string) =
        line.Trim() = ""

    let getValue (name: string) (m: Match) : string =
        m.Groups.[name].Captures.[0].Value

    let tryGetValue (name: string) (m: Match) : string option =
        try getValue name m |> Some
        with _ -> None

    let titlePattern = Regex("^(?<name>[\w'\- ]+)(\((?<desc>[\w,/ ]+)\))?\s*$")
    let tryParseTitle (line: string) : bool * DckTitle option =
        match titlePattern.Match line with
        | m when m.Success ->
            let title = {
                Name = (m |> getValue "name").Trim()
                Description = m |> tryGetValue "desc" |> Option.defaultValue ""
            }
            (true, Some title)
        | _ -> (false, None)

    let sectionHeaderPattern = Regex("^\.v(?<header>\w+)\s*$")
    let tryParseSectionHeader (line: string) : bool * string option =
        match sectionHeaderPattern.Match line with
        | m when m.Success ->
            let header = m |> getValue "header"
            (true, Some header)
        | _ -> (false, None)

    let cardPattern = Regex("^\.(?<id>\d+)\s+(?<count>\d+)\s+(?<name>.*)\s*$")
    let tryParseCard (line: string) : bool * DckCard option =
        match cardPattern.Match line with
        | m when m.Success ->
            let card = {
                Name = (m |> getValue "name").Trim()
                Id = m |> getValue "id" |> Int32.Parse
                Count = m |> getValue "count" |> Int32.Parse
            }
            (true, Some card)
        | _ -> (false, None)

    let parse (line: string) : Line option =
        match isBlank line with
        | true -> Some Blank
        | _ ->
            match tryParseTitle line with
            | (true, Some title) -> Title title |> Some
            | _ ->
                match tryParseSectionHeader line with
                | (true, Some header) -> SectionHeader header |> Some
                | _ ->
                    match tryParseCard line with
                    | (true, Some card) -> Card card |> Some
                    | _ -> None

let private popSection (cleanedLines: Line list) : (DckExtension option * Line list) =
    match cleanedLines with
    | [] -> (None, [])
    | (SectionHeader header)::tail ->
        let cards =
            tail
            |> List.takeWhile (fun x ->
                match x with
                | Card _ -> true
                | _ -> false
            )
            |> List.choose (fun x ->
                match x with
                | Card c -> Some c
                | _ -> None
            )

        Some {
            Name = header
            Cards = cards
        },
        tail |> List.skip cards.Length
    | _ -> raise <| FormatException("Section does not start with header")

let parse (dckText: string) : DckDeck =
    let lines =
        dckText.Split "\n"
        |> Seq.ofArray
        |> Seq.map Line.parse
        |> Seq.toList

    if lines |> List.exists Option.isNone then
        raise <| FormatException("One or more lines were not in a recognized format")

    let title =
        lines
        |> List.choose (fun x ->
            match x with
            | Some (Title x) -> Some x
            | _ -> None)
        |> List.tryHead
    if title.IsNone then
        raise <| FormatException("Title line missing")
    let title = title.Value

    // Remove blank lines and title
    let lines =
        lines
        |> List.filter (fun x ->
            match x with
            | Some Blank
            | Some (Title _)
            | None -> false
            | _ -> true)
        |> List.map Option.get

    // The core is all the cards before any section header
    let coreCards =
        lines
        |> List.takeWhile (fun x ->
            match x with
            | Card _ -> true
            | _ -> false
        )
        |> List.choose (fun x ->
            match x with
            | Card c -> Some c
            | _ -> None
        )
    let mutable lines = lines |> List.skip coreCards.Length

    let extensions = ResizeArray()
    let mutable stop = false
    while not stop do
        let (ext, remaining) = popSection lines
        match ext with
        | Some e ->
            extensions.Add e
            lines <- remaining
        | _ ->
            stop <- true
        ()

    {
        Name = title.Name
        Description = title.Description
        Cards = coreCards
        Extensions = extensions |> Seq.toList
    }

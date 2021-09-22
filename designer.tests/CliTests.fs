module GamesFaix.MtgTools.Designer.CliTests

open Xunit
open Argu
open System.Text.RegularExpressions
open System.Text

let single<'a when 'a :> IArgParserTemplate> (results: ParseResults<'a>) : 'a =
    let allResults = results.GetAllResults()
    Assert.Equal(1, allResults.Length)
    allResults.Head

let reduceWhitespace (str: string) : string =
    Regex.Replace(str, "\s+", " ")

let toArgs (command: string) =
    let result = ResizeArray()
    let words = command.Split(' ') |> Seq.rev |> System.Collections.Generic.Stack
    // Pull words from front of list until you hit one that starts with ".
    while words.Count > 0 do
        let w = words.Pop()
        if w.StartsWith '\"' then
            let sb = StringBuilder()
            sb.Append w |> ignore
            // Then combine with words until you hit one that ends with " or the end of the command
            let mutable closed = false
            while words.Count > 0 && (not closed) do
                let next = words.Pop()
                sb.Append $" {next}" |> ignore
                if next.EndsWith '\"' then
                    closed <- true
            let str = sb.ToString()
            let withoutQuotes = str.Substring(1, str.Length - 2);
            result.Add withoutQuotes
        else result.Add w
    result |> Seq.toArray

[<Fact>]
let ``toArgs works for one arg`` () =
    let expected = [|"foo"|]
    let actual = toArgs "foo"
    Assert.Equal<string[]>(expected, actual)

[<Fact>]
let ``toArgs works for many args`` () =
    let expected = [|"foo"; "bar"; "baz"|]
    let actual = toArgs "foo bar baz"
    Assert.Equal<string[]>(expected, actual)

[<Fact>]
let ``toArgs works for args with quotes`` () =
    let expected = [|"foo"; "-bar"; "baz biz buz"; "-boz"|]
    let actual = toArgs "foo -bar \"baz biz buz\" -boz"
    Assert.Equal<string[]>(expected, actual)

let parse (command: string) =
    let args = toArgs command
    let parser = ArgumentParser<Cli.Main.Args>(programName = "cli-tests")
    let result = parser.Parse(inputs = args, raiseOnUsage = true)
                 |> single
    result.ToString() |> reduceWhitespace

[<Theory>]
[<InlineData("workspace",
             "Workspace []")>]
[<InlineData("workspace -d some-folder",
             "Workspace [Dir \"some-folder\"]")>]
[<InlineData("login",
             "Login []")>]
[<InlineData("login -e test@test.com -p abc123",
             "Login [Email (Some \"test@test.com\"); Pass (Some \"abc123\")]")>]
[<InlineData("login -e test@test.com -p abc123 -s true",
             "Login [Email (Some \"test@test.com\"); Pass (Some \"abc123\"); SaveCreds (Some true)]")>]
[<InlineData("card copy -f ABC -t XYZ -n \"Lightning Bolt\"",
             "Card [Copy [FromSet \"ABC\"; ToSet \"XYZ\"; Name \"Lightning Bolt\"]]")>]
[<InlineData("card delete -s ABC -n \"Lightning Bolt\"",
             "Card [Delete [Set \"ABC\"; Name \"Lightning Bolt\"]]")>]
[<InlineData("card move -f ABC -t XYZ -n \"Lightning Bolt\"",
             "Card [Move [FromSet \"ABC\"; ToSet \"XYZ\"; Name \"Lightning Bolt\"]]")>]
[<InlineData("set audit ABC",
             "Set [Audit [Set \"ABC\"]]")>]
[<InlineData("set copy -f ABC -t XYZ",
             "Set [Copy [From \"ABC\"; To \"XYZ\"]]")>]
[<InlineData("set delete ABC",
             "Set [Delete [Set \"ABC\"]]")>]
[<InlineData("set pull ABC",
             "Set [Pull [Set \"ABC\"]]")>]
[<InlineData("set rename -f ABC -t XYZ",
             "Set [Rename [From \"ABC\"; To \"XYZ\"]]")>]
[<InlineData("set scrub ABC",
             "Set [Scrub [Set \"ABC\"]]")>]
let ``Parses input`` (input: string, expected: string) =
    let actual = parse input
    Assert.Equal(expected, actual)

module GamesFaix.MtgTools.Designer.CliTests

open Xunit
open Argu
open System.Text.RegularExpressions

let single<'a when 'a :> IArgParserTemplate> (results: ParseResults<'a>) : 'a =
    let allResults = results.GetAllResults()
    Assert.Equal(1, allResults.Length)
    allResults.Head

let reduceWhitespace (str: string) : string =
    Regex.Replace(str, "\s+", " ")

let parse (command: string) =
    let args = command.Split(' ')
    let parser = ArgumentParser<Cli.Main.Args>(programName = "cli-tests")
    let result = parser.Parse(inputs = args, raiseOnUsage = true)
                 |> single
    result.ToString() |> reduceWhitespace

[<Fact>]
let ``Workspace - Get`` () =
    let actual = parse $"workspace"
    let expected = $"Workspace []"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Workspace - Set`` () =
    let dir = "%userprofile%/desktop/foo"
    let actual = parse $"workspace -d {dir}"
    let expected = $"Workspace [Dir (Some \"{dir}\")]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Login with saved credentials`` () =
    let actual = parse $"login"
    let expected = $"Login []"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Login with credentials`` () =
    let email = "test@test.com"
    let pass = "abc123"
    let actual = parse $"login -e {email} -p {pass}"
    let expected = $"Login [Email (Some \"{email}\"); Pass (Some \"{pass}\")]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Login with credentials and save credentials`` () =
    let email = "test@test.com"
    let pass = "abc123"
    let actual = parse $"login -e {email} -p {pass} -s true"
    let expected = $"Login [Email (Some \"{email}\"); Pass (Some \"{pass}\"); SaveCreds (Some true)]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Card Copy`` () =
    let fromSet = "foo"
    let toSet = "bar"
    let name = "baz"
    let actual = parse $"card copy -f {fromSet} -t {toSet} -n {name}"
    let expected = $"Card [Copy [FromSet \"{fromSet}\"; ToSet \"{toSet}\"; Name \"{name}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Card Delete`` () =
    let set = "foo"
    let name = "baz"
    let actual = parse $"card delete -s {set} -n {name}"
    let expected = $"Card [Delete [Set \"{set}\"; Name \"{name}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Card Move`` () =
    let fromSet = "foo"
    let toSet = "bar"
    let name = "baz"
    let actual = parse $"card move -f {fromSet} -t {toSet} -n {name}"
    let expected = $"Card [Move [FromSet \"{fromSet}\"; ToSet \"{toSet}\"; Name \"{name}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Audit`` () =
    let set = "foo"
    let actual = parse $"set audit {set}"
    let expected = $"Set [Audit [Set \"{set}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Copy`` () =
    let fromSet = "foo"
    let toSet = "bar"
    let actual = parse $"set copy -f {fromSet} -t {toSet}"
    let expected = $"Set [Copy [From \"{fromSet}\"; To \"{toSet}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Delete`` () =
    let set = "foo"
    let actual = parse $"set delete {set}"
    let expected = $"Set [Delete [Set \"{set}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Pull`` () =
    let set = "foo"
    let actual = parse $"set pull {set}"
    let expected = $"Set [Pull [Set \"{set}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Rename`` () =
    let fromSet = "foo"
    let toSet = "bar"
    let actual = parse $"set rename -f {fromSet} -t {toSet}"
    let expected = $"Set [Rename [From \"{fromSet}\"; To \"{toSet}\"]]"
    Assert.Equal(expected, actual)

[<Fact>]
let ``Set Scrub`` () =
    let set = "foo"
    let actual = parse $"set scrub {set}"
    let expected = $"Set [Scrub [Set \"{set}\"]]"
    Assert.Equal(expected, actual)
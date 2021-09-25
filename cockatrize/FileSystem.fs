module GamesFaix.MtgTools.Dck2Cod.FileSystem

open System.IO
open System.Xml.Linq

let private createDirIfMissing (path: string) =
    let dir = Path.GetDirectoryName path
    match Directory.Exists dir with
    | false -> Directory.CreateDirectory dir |> ignore
    | _ -> ()

let writeCod (path: string) (cod: XDocument) : unit =
    createDirIfMissing path
    use stream = File.Open(path, FileMode.Create)
    cod.Save stream

let readText = File.ReadAllText

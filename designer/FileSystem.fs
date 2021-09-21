module GamesFaix.MtgTools.Designer.FileSystem

open System.IO
open Newtonsoft.Json

let createDirectoryIfMissing (path: string) : unit =
    if Directory.Exists path then ()
    else Directory.CreateDirectory path |> ignore

let deleteFolderIfExists (path: string) : unit =
    if Directory.Exists path
    then Directory.Delete(path, true)
    else ()

let saveFileBytes (bytes: byte[]) (path: string): unit Async =
    createDirectoryIfMissing (Path.GetDirectoryName path)
    File.WriteAllBytesAsync(path, bytes) |> Async.AwaitTask

let saveFileText (text: string) (path: string): unit Async =
    createDirectoryIfMissing (Path.GetDirectoryName path)
    File.WriteAllTextAsync(path, text) |> Async.AwaitTask

let loadFromJson<'a> (path: string) : 'a option Async =
    async {
        try
            let! json = File.ReadAllTextAsync path |> Async.AwaitTask
            let result = JsonConvert.DeserializeObject<'a> json
            return Some result
        with
        | _ ->
            return None
    }

let saveToJson<'a> (data: 'a) (path: string) : unit Async =
    let options = JsonSerializerSettings()
    options.Formatting <- Formatting.Indented
    let json = JsonConvert.SerializeObject(data, options)
    saveFileText json path

let deleteFilesInFolderMatching (dir: string) (filter : string -> bool) = async {
    let files =
        Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
        |> Seq.filter filter

    for f in files do
        File.Delete f

    return ()
}
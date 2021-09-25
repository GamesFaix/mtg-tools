module GamesFaix.MtgTools.Cockatrize.Tests.Data

open System.IO

let readDckFile (name: string) : string =
    let path = $"./TestData/{name}.dck"
    File.ReadAllText path
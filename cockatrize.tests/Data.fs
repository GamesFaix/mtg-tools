module GamesFaix.MtgTools.Dck2Cod.Tests.Data

open System.IO

let readDckFile (name: string) : string =
    let path = $"./TestData/{name}.dck"
    File.ReadAllText path
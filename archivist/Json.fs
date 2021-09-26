module GamesFaix.MtgTools.Archivist.Json

open System
open Microsoft.FSharp.Reflection
open Newtonsoft.Json

type OptionConverter() =
    inherit JsonConverter()
    
    override x.CanConvert(t) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

    override x.WriteJson(writer, value, serializer) =
        let value = 
            if value = null then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, value)

    override x.ReadJson(reader, t, existingValue, serializer) =        
        let innerType = t.GetGenericArguments().[0]
        let innerType = 
            if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
            else innerType        
        let value = serializer.Deserialize(reader, innerType)
        let cases = FSharpType.GetUnionCases(t)
        if value = null then FSharpValue.MakeUnion(cases.[0], [||])
        else FSharpValue.MakeUnion(cases.[1], [|value|])


let private converters : JsonConverter[] = 
    [| 
        OptionConverter() 
    |]

let private settings =
    let jss = JsonSerializerSettings()
    jss.Formatting <- Formatting.Indented
    for c in converters do
        jss.Converters.Add c
    jss

let serialize<'a> (data: 'a) : string =
    JsonConvert.SerializeObject(data, settings)

let deserialize<'a> (json: string): 'a =
    JsonConvert.DeserializeObject<'a>(json, converters)
module GamesFaix.MtgTools.Scry.Report

open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open System

let command (ctx: WorkspaceContext<Workspace.WorkspaceDirectory>) : CommandResult =
    async {
        ctx.Log.Information "Loading set information from Scryfall..."
        let! sets = Scryfall.getSets ()
        ctx.Log.Information $"  Found {sets.Length} results"

        let sets = 
            sets 
            |> Seq.filter (fun s -> 
                match s.SetType with
                | "core"
                | "expansion"
                | "masters"
                | "starter" -> true
                | _ -> false
            )
            |> Seq.sortBy (fun s -> s.ReleaseDate |> Option.ofNullable |> Option.defaultValue DateTime.MaxValue)
            |> Seq.toList

        ctx.Log.Information $"Filtered to {sets.Length} sets (core, expansion, starter, masters)"

        for s in sets do
            let name = s.Name.PadRight(40)

            let date = 
                if s.ReleaseDate.HasValue then
                    s.ReleaseDate.Value.ToString("MM/yy")
                else ""

            ctx.Log.Information $"{name} ({date})"

        return failwith ""
    }
open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Model
open System.IO

(* 
    mtg.design uses server side rendering, so no API available
    Load the page for a set, then parse the DOM to find links to each card in the set to get their IDs.
    Go to the edit page for each set to get structured data about each card, by reading the form
    Create in-memory index of (cardID, card name, mana cost, type)
    Sort cards and assign collector numbers
    Send request to server to update each card
    Make sure to click Center box for select cards becaues it is never set in the form when loading an existing card
*)

let block<'a> (task : 'a Task) : 'a = task.Result

[<EntryPoint>]
let main _ =
    task {
        let setName = "ECC"
        let readFromCache = false
        let useCachedImages = false
        let writeToCache = true
        let writeToWeb = false
        let mode = MtgDesignWriter.SaverMode.Edit

        printfn "Loading card details..."
        let! cards = task {
            if readFromCache
            then match! FileReaderWriter.loadJsonDetails setName with
                 | Some cards -> 
                    printfn "\tFound in cache."
                    return cards
                 | _ -> return! MtgDesignReader.getSetCardDetails setName
            else return! MtgDesignReader.getSetCardDetails setName
        }

        printfn "Processing cards..."
        let cards = Processor.processCards cards

        printfn "Auditing for issues..."
        let issues = Auditor.findIssues cards
        Auditor.printIssues issues

        if writeToCache
        then 
            printfn "Caching card details..."
            let! _ = FileReaderWriter.saveJsonDetails cards setName
            ()
        else ()

        if writeToWeb
        then 
          //  let cards = cards|> List.filter (fun c -> Int32.Parse(c.Number) > 18)
            printfn "Saving card details to mtg.design..."
            let! _ = MtgDesignWriter.saveCards mode cards
            ()
        else ()

        let cardInfos : CardInfo list = cards |> List.map (fun c -> 
            { 
                Name = c.Name
                Set = c.Set
                Id = c.Id
            })

        printfn "Downloading card images..."
        let! _ = cardInfos |> Utils.concurrentMap (fun c -> task {
            let path = FileReaderWriter.getCardImagePath c
            if useCachedImages && File.Exists path
            then
                printfn "\tFound cached image for %s..." c.Name
                return ()
            else
                printfn "\tDownloading image for %s..." c.Name
                let! bytes = MtgDesignReader.getCardImage c
                let! _ = FileReaderWriter.saveCardImage bytes c
                return ()    
        })

        //printfn "Creating HTML layout..."
        //let html = Layouter.createHtmlLayout cardInfos
        //let! _ = FileReaderWriter.saveHtmlLayout html setName

        //printfn "Creating PDF layout..."
        //let! pdf = Layouter.convertToPdf html
        //let! _ = FileReaderWriter.savePdfLayout pdf setName

        printfn "Done."
        return ()
    } |> block
    
    Console.Read() |> ignore
    0 // return an integer exit code

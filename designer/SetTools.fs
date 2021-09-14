// High-level commands for managing sets
module SetTools

open FSharp.Control.Tasks
open System.Threading.Tasks
open Model

let autonumberSet (setName: string) : unit Task =
    task {
        printfn "Auto-numbering %s..." setName
        let! cardDetails = MtgDesignReader.getSetCardDetails setName
        let processed = Processor.processCards cardDetails
        let! _ = MtgDesignWriter.saveCards MtgDesignWriter.SaverMode.Edit processed
        printfn "Done."
        return ()
    }

let renameSet (oldName : string) (newName: string) : unit Task =
    task {
        printfn "Renaming %s to %s..." oldName newName
        let! cardDetails = MtgDesignReader.getSetCardDetails oldName
        let processed = Processor.processCards cardDetails |> List.map (fun c -> { c with Set = newName })
        let! _ = MtgDesignWriter.saveCards MtgDesignWriter.SaverMode.Edit processed
        printfn "Done."
        return ()
    }

let cloneSet (oldName : string) (newName: string) : unit Task =
    task {
        printfn "Cloning %s to %s..." oldName newName
        let! cardDetails = MtgDesignReader.getSetCardDetails oldName
        let processed = Processor.processCards cardDetails |> List.map (fun c -> { c with Set = newName })
        let! _ = MtgDesignWriter.saveCards MtgDesignWriter.SaverMode.Create processed
        printfn "Done."
        return ()
    }

let deleteCard (setName : string) (cardName: string) : unit Task =
    task {
        printfn "Deleting %s - %s..." setName cardName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let card = cardInfos |> Seq.find (fun c -> c.Name = cardName)
        let! _ = MtgDesignWriter.deleteCard card        
        printfn "Done."
        return ()
    }


let deleteSet (setName : string) : unit Task =
    task {
        printfn "Deleting %s..." setName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let! _ = MtgDesignWriter.deleteCards cardInfos            
        printfn "Done."
        return ()
    }

let cloneCard (setName: string) (cardName: string) (newSetName: string) : unit Task =
    task {
        printfn "Cloning %s from %s to %s..." cardName setName newSetName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let card = cardInfos |> Seq.find (fun c -> c.Name = cardName)
        let! details = MtgDesignReader.getCardDetails card
        let details = Processor.processCard details
        let details = { details with Set = newSetName }
        let! _ = MtgDesignWriter.saveCards MtgDesignWriter.SaverMode.Create [details]
        printfn "Done."
        return()
    }

let private downloadCardImage (card: CardInfo) : unit Task =
    task {
        printfn "Downloading %s..." card.Name
        let! bytes = MtgDesignReader.getCardImage card
        let! _ = FileReaderWriter.saveCardImage bytes card
        return ()    
    }

let downloadSetImages (setName: string) : unit Task =
    task {
        printfn "Downloading images for %s..." setName
        FileReaderWriter.deleteSetFolder setName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let! _ = cardInfos |> Utils.concurrentMap downloadCardImage
        printfn "Done."
        return ()
    }

let createHtmlLayout (setName: string) : unit Task =
    task {
        printfn "Creating HTML layout for %s..." setName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let html = Layouter.createHtmlLayout cardInfos
        let! _ = FileReaderWriter.saveHtmlLayout html setName
        printfn "Done."
        return ()
    }

let createPdfLayout (setName: string) : unit Task =
    task {
        printfn "Creating PDF layout for %s..." setName
        let! cardInfos = MtgDesignReader.getSetCardInfos setName
        let html = Layouter.createHtmlLayout cardInfos
        let! bytes = Layouter.convertToPdf html 
        let! _ = FileReaderWriter.savePdfLayout bytes setName
        printfn "Done."       
        return ()
    }

let audit (setName: string) : unit Task =
    task {
        printfn "Auditing issues for %s..." setName
        let! cards = MtgDesignReader.getSetCardDetails setName
        let cards = Processor.processCards cards
        let issues = Auditor.findIssues cards
        Auditor.printIssues issues
        printfn "Done."
        return ()
    }
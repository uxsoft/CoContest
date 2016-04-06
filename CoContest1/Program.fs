module Program

open Problem
open System
open System.Collections.Generic
open System.Drawing
open System.Globalization
open System.IO
open System.Linq
open System.Text
open NaturalSelection
open System.Diagnostics

[<EntryPoint>]
let main argv = 
    let path = Array.head argv
    let pathAttr = File.GetAttributes(path)
    ()
    let files = 
        if pathAttr.HasFlag(FileAttributes.Directory) then Directory.EnumerateFiles(path, "*.txt")
        else seq { yield path }
    
    let processFile file = 
        let inputDirectory = Path.GetDirectoryName(file)
        let outputDirectory = Path.Combine(inputDirectory, "output")
        let outputFile = Path.Combine(outputDirectory, Path.GetFileName(file))
        try 
            let swatch = Stopwatch()
            swatch.Start()
            //let solution = DrykAssociation.run file
            let solution = Knapsacks.run file
            //let solution = DistanceMindedBruteForce.run file
            swatch.Stop()
            Console.WriteLine("---------------------------")
            Console.WriteLine(Path.GetFileNameWithoutExtension(file))
            Console.WriteLine("Finished in {0}", swatch.Elapsed)
            Console.WriteLine("---------------------------")
            File.WriteAllText(outputFile, solution)
        with e -> Console.WriteLine(e)
    
    let mutable nFinished = 0
    let nTotal = files.Count()
    Async.Parallel [ for file in files -> 
                         async { 
                             processFile file
                             nFinished <- nFinished + 1
                             Console.WriteLine("Finished {0}/{1}", nFinished, nTotal)
                             return ()
                         } ]
    |> Async.RunSynchronously
    |> ignore
    Console.WriteLine("=========================")
    Console.WriteLine("DONE: Press enter to exit")
    Console.WriteLine("=========================")
    Console.ReadLine() |> ignore
    //NaturalSelection.run (Array.head argv)
    0

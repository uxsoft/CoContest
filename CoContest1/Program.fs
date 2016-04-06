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

let loadScore file = 
    try 
        use s = File.OpenRead(file)
        use sr = new StreamReader(s)
        Double.Parse(sr.ReadLine(), CultureInfo.InvariantCulture)
    with e -> 0.0

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
            Console.WriteLine("---------------------------")
            let swatch = Stopwatch()
            swatch.Start()
            let problem = loadProblem file
            let solution = DrykAssociation.run problem
            swatch.Stop()
            Console.WriteLine(Path.GetFileNameWithoutExtension(file))
            Console.WriteLine("Finished in {0}", swatch.Elapsed)
            let existingScore = loadScore outputFile
            let newScore = rank problem solution
            if existingScore > newScore then 
                Console.WriteLine("NEW HIGHSCORE! {0} -> {1}", existingScore, newScore)
                File.WriteAllText(outputFile, sprintResult solution newScore)
            Console.WriteLine("---------------------------")
        with e -> Console.WriteLine(e)
    
    let mutable nFinished = 0
    let nTotal = files.Count()
    while true do
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

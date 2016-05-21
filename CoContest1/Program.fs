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
    with e -> Double.MaxValue

[<EntryPoint>]
let main argv = 
    let path = Array.head argv
    let pathAttr = File.GetAttributes(path)
    ()
    let files = 
        if pathAttr.HasFlag(FileAttributes.Directory) then Directory.EnumerateFiles(path, "*.txt")
        else seq { yield path }
    
    let problems = files |> Seq.map (fun f -> (f, loadProblem f)) |> Seq.toArray
    
    let processFile file problem = 
        let inputDirectory = Path.GetDirectoryName(file)
        let outputDirectory = Path.Combine(inputDirectory, "output")
        let outputFile = Path.Combine(outputDirectory, Path.GetFileName(file))
        try 
            let swatch = Stopwatch()
            //Console.WriteLine("---------------------------")
            //Console.WriteLine(Path.GetFileNameWithoutExtension(file))
            swatch.Start()
            let solutions = AssociationNReflow.run problem
            for solution in solutions do
                let existingScore = loadScore outputFile
                let newScore = rank problem solution
                if existingScore > newScore + Double.Epsilon then 
                    Console.ForegroundColor <- ConsoleColor.Green
                    Console.WriteLine
                        ("NEW HIGHSCORE! {0} -> {1} ({2}%)", existingScore, newScore, newScore / existingScore * 100.0)
                    Console.ForegroundColor <- ConsoleColor.White
                    File.WriteAllText(outputFile, sprintResult solution newScore)
            swatch.Stop()
        //Console.WriteLine("Finished in {0}", swatch.Elapsed)
        //Console.WriteLine("---------------------------")
        with e -> Console.WriteLine(e.Message)
    
    let mutable nFinished = 0
    let mutable nGeneration = 0
    let nTotal = files.Count()
    while true do
        Async.Parallel [ for (file, problem) in problems -> 
                             async { 
                                 processFile file problem
                                 nFinished <- nFinished + 1
                                 //Console.WriteLine("Finished {0}/{1} Generation {2}", nFinished, nTotal, nGeneration)
                                 return ()
                             } ]
        |> Async.RunSynchronously
        |> ignore
        nFinished <- 0
        nGeneration <- nGeneration + 1
        Console.WriteLine("Generation: {0}", nGeneration)
    //Console.WriteLine("=========================")
    //Console.WriteLine("DONE: Press enter to exit")
    //Console.WriteLine("=========================")
    Console.ReadLine() |> ignore
    //NaturalSelection.run (Array.head argv)
    0

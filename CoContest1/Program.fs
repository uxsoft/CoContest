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

[<EntryPoint>]
let main argv = 
    let path = Array.head argv
    let pathAttr = File.GetAttributes(path)
    ()
    let files = 
        if pathAttr.HasFlag(FileAttributes.Directory) then Directory.EnumerateFiles(path, "*.txt")
        else seq { yield path }
    for file in files do
        let inputDirectory = Path.GetDirectoryName(file)
        let outputDirectory = Path.Combine(inputDirectory, "output")
        let outputFile = Path.Combine(outputDirectory, Path.GetFileName(file))
        try
            Console.WriteLine("---------------------------")
            Console.WriteLine(Path.GetFileNameWithoutExtension(file))
            Console.WriteLine("---------------------------")
            //let solution = NaturalSelection.run file
            let solution = DrykAssociation.run file
            File.WriteAllText(outputFile, solution)
            Console.WriteLine(solution)
        with e -> ()
    Console.ReadLine() |> ignore
    //NaturalSelection.run (Array.head argv)
    0

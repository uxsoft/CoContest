module Program

open System
open System.Drawing
open System.IO
open System.Globalization
open System.Text
open System.Linq
open System.Collections.Generic

type Chromosome = int array

let sprintResult (chromosome : Chromosome) (rank : float) = 
    let sb = StringBuilder()
    sb.Append("Chromosome: [|") |> ignore
    for f in chromosome do
        sb.AppendFormat("{0}; ", f) |> ignore
    sb.AppendLine("|]") |> ignore
    sb.AppendLine("----------------------------") |> ignore
    sb.AppendLine(sprintf "%.10f" rank) |> ignore
    let lines = 
        chromosome
        |> Seq.mapi (fun i v -> KeyValuePair(v, i))
        |> Seq.groupBy (fun kvp -> kvp.Key)
        |> Seq.sortBy (fun (f, values) -> f)
        |> Seq.map 
               (fun (f, customers) -> sprintf "%d %s" f (String.Join(" ", customers |> Seq.map (fun kvp -> kvp.Value))))
    sb.Append(String.Join("\n", lines)) |> ignore
    sb.ToString()

type Population = Chromosome seq

let initialPopulation (random : Random) nCustomers nFacilities n = 
    seq { 
        for i in 1..n do
            yield seq { 
                      for c in 1..nCustomers do
                          yield random.Next(nFacilities)
                  }
                  |> Seq.toArray
    }

let distance ((aX, aY) : float * float) ((bX, bY) : float * float) = 
    Math.Sqrt(float ((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY)))

let validate (demands : float array) (capacities : float array) (c : Chromosome) = 
    c
    |> Array.mapi (fun i f -> (f, i))
    |> Array.groupBy (fun (f, i) -> f)
    |> Array.map (fun (f, customers) -> 
           let demand = 
               customers
               |> Array.map (fun (f, i) -> demands.[i])
               |> Array.sum
           capacities.[f] >= demand)
    |> Array.reduce (fun a b -> a && b)

let rank (distances : float [,]) (costs : float array) (isValid : Chromosome -> bool) (c : Chromosome) = 
    let distancesCost = 
        c
        |> Array.mapi (fun c f -> distances.[f, c])
        |> Array.sum
    
    let constructionCost = 
        c
        |> Array.distinct
        |> Array.map (fun f -> costs.[f])
        |> Array.sum
    
    if isValid c then distancesCost + float constructionCost
    else Double.MaxValue

let mutate (random : Random) (mutationRate : double) (nFacilities : int) (a : Chromosome) = 
    if random.NextDouble() > mutationRate then 
        let index = random.Next(a.Length)
        let newValue = random.Next(nFacilities)
        a.[index] <- newValue
    a

let clone (a : Chromosome) = a.Clone() :?> Chromosome

let crossover (random : Random) (crossoverRate : double) (a : Chromosome) (b : Chromosome) = 
    if random.NextDouble() > crossoverRate then 
        let splitIndex = random.Next(a.Length)
        let (a1, a2) = Array.splitAt splitIndex a
        let (b1, b2) = Array.splitAt splitIndex b
        let aChild = Array.append a1 b2
        let bChild = Array.append b1 a2
        (aChild, bChild)
    else (clone a, clone b)

let pick (random : Random) (rankedPopulation : (Chromosome * float) seq) = 
    let picked = 
        rankedPopulation
        |> Seq.sortBy (fun (c, r) -> r * random.NextDouble())
        |> Seq.head
    match picked with
    | (c, r) -> c

let nextGeneration (rank : Chromosome -> float) (crossover : Chromosome -> Chromosome -> (Chromosome * Chromosome)) 
    (mutate : Chromosome -> Chromosome) (pick : (Chromosome * float) seq -> Chromosome) (n : int) 
    (callback : Chromosome * float -> unit) (population : Population) = 
    let rankedPopulation = 
        population
        |> Seq.map (fun c -> (c, rank c))
        |> Seq.sortBy (fun (c, r) -> r)
    callback (Seq.head rankedPopulation)
    seq { 
        for i in 1..(n / 2) do
            let a = pick rankedPopulation
            let b = pick rankedPopulation
            let (a2, b2) = crossover a b
            yield mutate a2
            yield mutate b2
    }
    |> Seq.toArray

[<EntryPoint>]
let main argv = 
    //Input
    let input = 
        File.ReadAllLines(Array.head argv) 
        |> Array.map (fun s -> s.Split(' ') |> Array.map (fun s -> Double.Parse(s, CultureInfo.InvariantCulture)))
    //Setup
    let numFacilities = int input.[0].[0]
    let numCustomers = int input.[0].[1]
    let costs = Array.create numFacilities 0.0
    let capacities = Array.create numFacilities 0.0
    let demands = Array.create numCustomers 0.0
    let customerPositions = Array.create numCustomers (0.0, 0.0)
    let facilityPositions = Array.create numFacilities (0.0, 0.0)
    let distances = Array2D.create numFacilities numCustomers 0.0
    //
    input
    |> Array.skip (1)
    |> Array.take (numFacilities)
    |> Array.iteri (fun i s -> 
           costs.[i] <- s.[0]
           capacities.[i] <- s.[1]
           facilityPositions.[i] <- (s.[2], s.[3]))
    //
    input
    |> Array.skip (numFacilities + 1)
    |> Array.take numCustomers
    |> Array.iteri (fun i s -> 
           demands.[i] <- s.[0]
           customerPositions.[i] <- (s.[1], s.[2]))
    //
    let a = distance customerPositions.[0] facilityPositions.[0]
    for f in 0..numFacilities - 1 do
        for c in 0..numCustomers - 1 do
            distances.[f, c] <- distance customerPositions.[c] facilityPositions.[f]
    //GA Setup
    let random = new Random()
    let aValidate = validate demands capacities
    let aRank = rank distances costs aValidate
    //
    let test = 
        aRank 
            [| 6; 14; 3; 15; 2; 1; 12; 18; 8; 7; 13; 22; 4; 0; 16; 12; 8; 11; 10; 11; 20; 6; 4; 16; 20; 17; 7; 1; 10; 14; 
               10; 23; 8; 18; 24; 7; 5; 20; 15; 15; 4; 12; 7; 15; 6; 20; 19; 15; 15; 6; 2; 21; 1; 23; 7; 23; 8; 8; 3; 22 |]
    //
    let aMutate = mutate random 0.2 numFacilities
    let aCrossover = crossover random 0.7
    let aPick = pick random
    let populationSize = 2 * (numCustomers + numFacilities)
    let mutable bestChromosome = [||]
    let mutable bestChromosomeRank = Double.MaxValue
    
    let aCallback = 
        (fun (c, r) -> 
        if r < bestChromosomeRank then 
            bestChromosomeRank <- r
            bestChromosome <- c
            Console.Clear()
            Console.SetCursorPosition(0, 1)
            Console.Write(sprintResult c r))
    
    let aNextGeneration = nextGeneration aRank aCrossover aMutate aPick populationSize aCallback
    let mutable population = initialPopulation random numCustomers numFacilities populationSize
    let mutable generation = 0
    //GA
    while true do
        population <- aNextGeneration population
        generation <- generation + 1
        Console.SetCursorPosition(0, 0)
        printf "Generation: %d" generation
    0

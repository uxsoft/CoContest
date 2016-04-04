module Program

open System
open System.Drawing
open System.IO

type Chromosome = int array

type Population = Chromosome seq

let initialPopulation nCustomers nFacilities n = 
    let random = new Random()
    seq { 
        for i in 1..n do
            yield Array.create nCustomers (random.Next(nFacilities))
    }

let distance (a : PointF) (b : PointF) = Math.Sqrt(float ((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y)))

let rank (distances : float array array) (costs : float32 array) (demands : float32 array) (capacities : float32 array) (c : Chromosome) = 
    let distancesCost = 
        c
        |> Array.mapi (fun c f -> distances.[f].[c])
        |> Array.sum
    
    let constructionCost = 
        c
        |> Array.distinct
        |> Array.map (fun f -> costs.[f])
        |> Array.sum
    
    let isValid = 
        c
        |> Array.mapi (fun c f -> (f, c))
        |> Array.groupBy (fun (f, c) -> f)
        |> Array.map (fun (f, customers) -> 
               let demand = 
                   customers
                   |> Array.map (fun (f, c) -> demands.[c])
                   |> Array.sum
               capacities.[f] <= demand)
        |> Array.reduce (fun a b -> a && b)
    
    if isValid then distancesCost + float constructionCost
    else Double.MaxValue

let mutate (random : Random) (mutationRate : double) (nFacilities : int) (a : Chromosome) = 
    if random.NextDouble() > mutationRate then 
        let index = random.Next(a.Length)
        let newValue = random.Next(nFacilities)
        a.[index] <- newValue
        a
    else a

let clone (a : Chromosome) = a.Clone() :?> Chromosome

//    let b = Array.zeroCreate a.Length
//    a |> Array.iteri (fun i v -> b.[i] <- v)
//    b
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
    let total = rankedPopulation |> Seq.sumBy (fun (c, r) -> r)
    let mutable r = random.NextDouble()
    let mutable picked = [||]
    for (c, rank) in rankedPopulation do
        let ratio = rank / total
        if r <= ratio then picked <- c
        else r <- r - ratio
    picked

let nextGeneration (rank : Chromosome -> float) (crossover : Chromosome -> Chromosome -> (Chromosome * Chromosome)) 
    (mutate : Chromosome -> Chromosome) (pick : (Chromosome * float) seq -> Chromosome) (n : int) (callback : Chromosome * float -> unit) 
    (population : Population) = 
    let rankedPopulation = 
        population
        |> Seq.map (fun c -> (c, rank c))
        |> Seq.sortBy (fun (c, r) -> r)
    callback(Seq.head rankedPopulation)
    seq { 
        for i in 1..(n / 2) do
            let a = pick rankedPopulation
            let b = pick rankedPopulation
            let (a2, b2) = crossover a b
            yield mutate a2
            yield mutate b2
    }

[<EntryPoint>]
let main argv = 
    //Input
    let input = File.ReadAllLines(Array.head argv) |> Array.map (fun s -> s.Split(' ') |> Array.map Single.Parse)
    //Setup
    let numFacilities = int input.[0].[0]
    let numCustomers = int input.[0].[1]
    let costs = Array.create numFacilities 0.0f
    let capacities = Array.create numFacilities 0.0f
    let demands = Array.create numCustomers 0.0f
    let customerPositions = Array.create numCustomers (new PointF(0.0f, 0.0f))
    let facilityPositions = Array.create numFacilities (new PointF(0.0f, 0.0f))
    let distances = Array.create numFacilities (Array.create numCustomers 0.0)
    //
    input
    |> Array.skip (1)
    |> Array.take (numFacilities)
    |> Array.iteri (fun i s -> 
           costs.[i] <- s.[0]
           capacities.[i] <- s.[1]
           facilityPositions.[i] <- PointF(s.[2], s.[3]))
    //
    input
    |> Array.skip (numFacilities + 1)
    |> Array.take numCustomers
    |> Array.iteri (fun i s -> 
           demands.[i] <- s.[0]
           customerPositions.[i] <- PointF(s.[1], s.[2]))
    //
    for f in 0..numFacilities - 1 do
        for c in 0..numCustomers - 1 do
            distances.[f].[c] <- distance customerPositions.[c] facilityPositions.[f]
    //GA Setup
    let random = new Random()
    let aRank = rank distances costs demands capacities
    let aMutate = mutate random 0.1 numFacilities
    let aCrossover = crossover random 0.7
    let aPick = pick random
    let populationSize = (numCustomers + numFacilities)
    let mutable bestChromosome = [||]
    let mutable bestChromosomeRank = Double.MaxValue
    
    let aCallback = 
        (fun (c, r) -> 
        if r < bestChromosomeRank then 
            bestChromosomeRank <- r
            bestChromosome <- c)
    
    let aNextGeneration = nextGeneration aRank aCrossover aMutate aPick populationSize aCallback
    let mutable population = initialPopulation numCustomers numFacilities populationSize
    //GA
    while true do
        population <- aNextGeneration population
    0

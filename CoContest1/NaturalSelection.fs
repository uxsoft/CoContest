module NaturalSelection

open Problem
open System

type Population = Chromosome seq

let initialPopulation (random : Random) (p:Problem) (isValid: Chromosome->bool) n = 
    seq { 
        while true do
            yield seq { 
                      for c in 1..p.NumberOfCustomers do
                          yield random.Next(p.NumberOfFacilities)
                  }
                  |> Seq.toArray
    }
    |> Seq.filter isValid
    |> Seq.take n
    |> Seq.toArray


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

let run file =
    //Input
    let problem = loadProblem (file)
    //GA Setup
    let random = new Random()
    let aValidate = validate problem
    let aRank = rank problem aValidate
    //
    
    let aMutate = mutate random 0.2 problem.NumberOfFacilities
    let aCrossover = crossover random 0.7
    let aPick = pick random
    let populationSize = 2 * (problem.NumberOfCustomers+problem.NumberOfFacilities)
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
    let mutable population = initialPopulation random problem aValidate populationSize
    let mutable generation = 0
    //GA
    while true do
        population <- aNextGeneration population
        generation <- generation + 1
        Console.SetCursorPosition(0, 0)
        printf "Generation: %d" generation
    0
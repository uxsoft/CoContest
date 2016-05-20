module NaturalSelection

open Problem
open System
open System.Linq

type Population = Chromosome seq

let initialPopulation (random : Random) (p : Problem) (isValid : Chromosome -> bool) n = 
    Console.WriteLine("Generating initial  population...")
    let generator = seq { 
        while true do
            yield seq { 
                      for c in 1..p.NumberOfCustomers do
                          yield random.Next(p.NumberOfFacilities)
                  }
                  |> Seq.toArray
    }
    generator.Take(n)
             .ToArray()

let mutate (random : Random) (isValid : Chromosome -> bool) (p : Problem) (a : Chromosome) = 
    let aChild = clone a
    let index = random.Next(aChild.Length)
    let newValue = random.Next(p.NumberOfFacilities)
    aChild.[index] <- newValue
    if isValid aChild then aChild
    else a

let crossover (random : Random) (isValid : Chromosome -> bool) (a : Chromosome) (b : Chromosome) = 
    let splitIndex = random.Next(a.Length)
    let (a1, a2) = Array.splitAt splitIndex a
    let (b1, b2) = Array.splitAt splitIndex b
    let aChild = Array.append a1 b2
    let bChild = Array.append b1 a2
    
    let aResult = 
        if isValid aChild then aChild
        else clone a
    
    let bResult = 
        if isValid bChild then bChild
        else clone b
    
    (aResult, bResult)

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
    let aRank = rank problem
    //
    let aMutate = mutate random aValidate problem
    let aCrossover = crossover random aValidate
    let aPick = pick random
    let populationSize = 2 * (problem.NumberOfCustomers + problem.NumberOfFacilities)
    let mutable bestChromosome = [||]
    let mutable bestChromosomeRank = Double.MaxValue
    
    let aCallback = 
        (fun (c, r) -> 
        if r < bestChromosomeRank then 
            bestChromosomeRank <- r
            bestChromosome <- c)
    
    let aNextGeneration = nextGeneration aRank aCrossover aMutate aPick populationSize aCallback
    let mutable population = initialPopulation random problem aValidate populationSize
    //GA
    for generation in [1..1000] do
        population <- aNextGeneration population
        Console.SetCursorPosition(0, Console.CursorTop)
        printf "Generation: %d" generation

    sprintResult bestChromosome bestChromosomeRank

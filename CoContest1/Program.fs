module Program

open System
open System.Drawing
open System.IO

type Chromosome = int array

let distance (a : PointF) (b : PointF) = Math.Sqrt(float ((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y)))

let rank (c : Chromosome) (distances : float array array) (costs : float32 array) (demands : float32 array) (capacities : float32 array) = 
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

let mutate = ()
let crossover = ()

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
    0

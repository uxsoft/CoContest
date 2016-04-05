module Problem

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Text

type Chromosome = int array

type Problem = 
    { NumberOfFacilities : int;
      NumberOfCustomers : int;
      Costs : float [];
      Capacities : float [];
      Demands : float [];
      Distances : float [,] }

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
        |> Seq.map (fun (f, customers) -> sprintf "%d %s" f (String.Join(" ", customers |> Seq.map (fun kvp -> kvp.Value))))
    sb.Append(String.Join("\n", lines)) |> ignore
    sb.ToString()

let distance ((aX, aY) : float * float) ((bX, bY) : float * float) = Math.Sqrt(float ((aX - bX) * (aX - bX) + (aY - bY) * (aY - bY)))

let validate (p:Problem) (c : Chromosome) = 
    c
    |> Array.mapi (fun i f -> (f, i))
    |> Array.groupBy (fun (f, i) -> f)
    |> Array.map (fun (f, customers) -> 
           let demand = 
               customers
               |> Array.map (fun (f, i) -> p.Demands.[i])
               |> Array.sum
           p.Capacities.[f] >= demand)
    |> Array.reduce (fun a b -> a && b)

let rank (p:Problem) (isValid : Chromosome -> bool) (c : Chromosome) = 
    let distancesCost = 
        c
        |> Array.mapi (fun c f -> p.Distances.[f, c])
        |> Array.sum
    
    let constructionCost = 
        c
        |> Array.distinct
        |> Array.map (fun f -> p.Costs.[f])
        |> Array.sum
    
    if isValid c then distancesCost + float constructionCost
    else Double.MaxValue

let loadProblem file = 
    let input = 
        File.ReadAllLines(file) |> Array.map (fun s -> s.Split(' ') |> Array.map (fun s -> Double.Parse(s, CultureInfo.InvariantCulture)))
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
    { NumberOfFacilities = numFacilities;
      NumberOfCustomers = numCustomers;
      Costs = costs;
      Capacities = capacities;
      Demands = demands;
      Distances = distances }
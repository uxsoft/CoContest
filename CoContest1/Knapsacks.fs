module Knapsacks

open Problem
open System
open System.Collections.Generic
open CoContest.Knapsacks

let run (problem : Problem) = 
    let customers = problem.Demands |> Array.mapi (fun i d -> Customer(i, d))
    let facilities = problem.Capacities |> Array.mapi (fun i c -> Facility(i, c))
    let solutions = KnapsacksDecomposition.DivideAndConquer(facilities, customers)
    solutions
    |> Seq.where (fun s -> validate problem s)
    |> Seq.sortBy (fun s -> rank problem s)
    |> Seq.head

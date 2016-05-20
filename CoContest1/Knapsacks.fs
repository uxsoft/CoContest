module Knapsacks

open Problem
open System
open System.Collections.Generic
open CoContest.Knapsacks

let run (problem:Problem) =
    let customers = problem.Demands |> Array.mapi (fun i d -> Customer(i, d))
    let facilities = problem.Capacities |> Array.mapi (fun i c -> Facility(i, c))
    
    
    //let solution = KnapsacksDecomposition.OptimizedDivideAndConquer(facilities, customers, problem.Distances);
    let solution = SolutionX.Solve(facilities, customers, problem.Distances);
    
    solution 
    |> Seq.filter (fun s -> validate problem s)
    
    //let solutions = KnapsacksDecomposition.DivideAndConquer(facilities, customers)
    //solutions

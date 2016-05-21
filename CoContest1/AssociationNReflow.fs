module AssociationNReflow

open Problem
open System
open System.Collections.Generic
open CoContest.Models
open CoContest.DrykAssociation

let run (problem:Problem) =
    let customers = problem.Demands |> Array.mapi (fun i d -> Customer(i, d))
    let facilities = problem.Capacities |> Array.mapi (fun i c -> Facility(i, c, problem.Costs.[i]))
    
    
    let solution = FacilitySubsetNAssociation.Solve(facilities, customers, problem.Distances);
    
    solution 
    |> Seq.filter (fun s -> validate problem s)

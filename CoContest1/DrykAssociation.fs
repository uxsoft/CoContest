module DrykAssociation

open Problem
open System.Collections.Generic
open System

type FacilityDistance = 
    { Facility : int;
      Distance : double }

let findSolution (problem : Problem) = 
    //naively assign customers    
    let solution = Array.zeroCreate problem.NumberOfCustomers
    let remainingCapacity = Array.create problem.NumberOfFacilities 0.0
    for facility in [ 0..problem.NumberOfFacilities - 1 ] do
        remainingCapacity.[facility] <- problem.Capacities.[facility]
    //
    for customer in [ 0..problem.NumberOfCustomers - 1 ] do
        let facilities = 
            seq { 
                for facility in 0..problem.NumberOfFacilities - 1 do
                    yield { Distance = problem.Distances.[customer, facility];
                            Facility = facility }
            }
            |> Seq.sortBy (fun fd -> fd.Distance)
            |> Seq.toList
        
        let canAssign (fd : FacilityDistance) = 
            let demand = problem.Demands.[customer]
            let availableCapacity = remainingCapacity.[fd.Facility]
            demand <= availableCapacity
        
        let rec assign (list : FacilityDistance list) = 
            if canAssign list.Head then 
                let demand = problem.Demands.[customer]
                let facility = list.Head.Facility
                solution.[customer] <- facility
                remainingCapacity.[facility] <- remainingCapacity.[facility] - demand
            else assign list.Tail
        
        assign facilities
    let naiveSolutionRank = rank problem solution
    Console.WriteLine("Valid?: {0}", validate problem solution)
    sprintResult solution naiveSolutionRank

let run (file : string) = 
    let problem = loadProblem file
    findSolution problem

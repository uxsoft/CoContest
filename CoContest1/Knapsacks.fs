module Knapsacks

open Problem
open System
open MilpManager.Abstraction
open System.Collections.Generic

type Item = 
    { Customer : int;
      Demand : float }

let knapsack (options : Item seq) (capacity : float) = 
    let solver = OrToolsMilpManager.Implementation.OrToolsMilpSolver(Int32.MaxValue)
    let vars = Dictionary<int, IVariable>()
    let weightedVars = Dictionary<int, IVariable>()
    for option in options do
        let var = solver.Create(sprintf "c%d" option.Customer, Domain.BinaryInteger)
        vars.[option.Customer] <- var
        weightedVars.[option.Customer] <- solver.MultiplyVariableByConstant
                                              (var, solver.FromConstant(option.Demand), Domain.AnyReal)
    let objective = solver.Operation(OperationType.Addition, weightedVars.Values |> Seq.toArray)
    solver.SetEqual(objective, solver.FromConstant(capacity))
    solver.AddGoal("capacity", objective)
    solver.Solve()
    let success = solver.GetStatus() <> SolutionStatus.Infeasible
    if not success then failwith "failed building knapsack"
    options
    |> Seq.map (fun o -> o.Customer)
    |> Seq.filter (fun customer -> solver.GetValue(vars.[customer]) = 1.0)

let tryBuildKnapsacks problem (random : Random) = 
    let solution = Array.create problem.NumberOfCustomers 0
    
    let mutable options = 
        problem.Demands
        |> Array.sortBy (fun f -> random.NextDouble())
        |> Array.mapi (fun i d -> 
               { Customer = i;
                 Demand = d })
    for facility in [ 0..problem.NumberOfFacilities - 1 ] |> List.sortBy (fun f -> random.NextDouble()) do
        let result = knapsack options problem.Capacities.[facility]
        for customer in result do
            solution.[customer] <- facility
        options <- options
                   |> Seq.filter (fun o -> result |> Seq.contains o.Customer)
                   |> Seq.toArray
    solution

let run file = 
    let problem = loadProblem file
    let random = Random()
    let totalCapacity = problem.Capacities |> Array.sum
    let totalDemand = problem.Demands |> Array.sum
    let mutable solution = Array.create problem.NumberOfCustomers 0
    while not <| validate problem solution do
        try 
            solution <- tryBuildKnapsacks problem random
        with e -> ()
    let solutionRank = rank problem solution
    sprintResult solution solutionRank

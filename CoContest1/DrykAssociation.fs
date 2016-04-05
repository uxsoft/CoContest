module DrykAssociation

open Problem
open System.Collections.Generic
open System

type FacilityDistance = 
    { Facility : int;
      Distance : double }

let rec assign (problem:Problem) (solution:Chromosome) (customer:int) (options : FacilityDistance list) (remainingCapacity: float []) = 
            
    let canAssign (fd : FacilityDistance) = 
        let demand = problem.Demands.[customer]
        let availableCapacity = remainingCapacity.[fd.Facility]
        demand <= availableCapacity
            
    if canAssign options.Head then 
        let demand = problem.Demands.[customer]
        let facility = options.Head.Facility
        solution.[customer] <- facility
        remainingCapacity.[facility] <- remainingCapacity.[facility] - demand
        true
    else  if options.Length > 1 then 
            assign problem solution customer options.Tail remainingCapacity
          else false

let findSolution (problem : Problem) = 
    let random = new Random()
    //naively assign customers    
    let mutable solution = Array.zeroCreate problem.NumberOfCustomers
    let remainingCapacity = Array.create problem.NumberOfFacilities 0.0
    for facility in [ 0..problem.NumberOfFacilities - 1 ] do
        remainingCapacity.[facility] <- problem.Capacities.[facility]
    //
    let homelessCustomers = List<int>()
    for customer in  [0..problem.NumberOfCustomers - 1] |> List.sortBy (fun f-> random.NextDouble()) do
        let facilities = 
            seq { 
                for facility in 0..problem.NumberOfFacilities - 1 do
                    yield { Distance = problem.Distances.[customer, facility];
                            Facility = facility }
            }
            |> Seq.sortBy (fun fd -> fd.Distance)
            |> Seq.toList
        
        if not <| assign problem solution customer facilities remainingCapacity then
            homelessCustomers.Add(customer)

    //TODO handle the homeless

    let naiveSolutionRank = rank problem solution
    //Redistribute loners to avoid building unnecessary facilities 
    let redistributionThreshold = 1
    let redistributionOptions  = solution
                                   |> Seq.mapi (fun c f -> KeyValuePair(f, c))
                                   |> Seq.groupBy (fun kvp -> kvp.Key)
                                   |> Seq.filter (fun (f, cs) -> cs|> Seq.length > redistributionThreshold)
                                   |> Seq.map (fun (f, cs) -> f)

    let redistributionCandidates = solution
                                   |> Seq.mapi (fun c f -> KeyValuePair(f, c))
                                   |> Seq.groupBy (fun kvp -> kvp.Key)
                                   |> Seq.filter (fun (f,cs) -> cs|> Seq.length <= redistributionThreshold)
                                   |> Seq.sortByDescending (fun (f, cs) -> problem.Costs.[f]) 
                                   |> Seq.map (fun (f, cs) -> cs |> Seq.map (fun kvp -> kvp.Value))

    for facilityCustomers in redistributionCandidates do
        try
            let relocatedSolution = clone solution
            let remainingCapacityAfterRelocation = remainingCapacity.Clone() :?> float[]
            
            for movingCustomer in facilityCustomers do
                let orderedRelocationOptions = redistributionOptions 
                                               |> Seq.map (fun f -> {Facility=f; Distance=problem.Distances.[movingCustomer,f]})
                                               |> Seq.sortBy(fun fd -> fd.Distance)
                                               |> Seq.toList
                assign problem relocatedSolution movingCustomer orderedRelocationOptions remainingCapacityAfterRelocation |> ignore
            if validate problem relocatedSolution then
                if rank problem relocatedSolution > naiveSolutionRank then
                    solution <- relocatedSolution

        with e -> ()

    Console.WriteLine("Valid?: {0}", validate problem solution)
    sprintResult solution naiveSolutionRank

let run (file : string) = 
    let problem = loadProblem file
    findSolution problem

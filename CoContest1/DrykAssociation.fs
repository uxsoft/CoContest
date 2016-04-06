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

let run (problem : Problem) = 
    let random = new Random()
    //naively assign customers    
    let mutable solution = Array.zeroCreate problem.NumberOfCustomers
    let remainingCapacity = problem.Capacities.Clone() :?> float []
    //
    let homelessCustomers = List<int>()

    for customer in  [0..problem.NumberOfCustomers - 1] |> List.sortBy (fun c -> random.NextDouble()) do
        let facilities = 
            seq { 
                for facility in 0..problem.NumberOfFacilities - 1 do
                    yield { Distance = problem.Distances.[customer, facility];
                            Facility = facility }
            }
            |> Seq.sortBy (fun fd -> fd.Distance * random.NextDouble())
            |> Seq.toList
        
        if not <| assign problem solution customer facilities remainingCapacity then
            homelessCustomers.Add(customer)
         
    
    //Handle homeless
    if homelessCustomers.Count > 0 then
        failwith "houston we have homeless"

    let naiveSolutionRank = rank problem solution
    //Redistribute loners to avoid building unnecessary facilities 
    let redistributionThreshold = 2
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
                                               |> Seq.map (fun f -> {Facility = f; Distance = problem.Distances.[movingCustomer, f]})
                                               |> Seq.sortBy(fun fd -> fd.Distance)
                                               |> Seq.toList
                assign problem relocatedSolution movingCustomer orderedRelocationOptions remainingCapacityAfterRelocation |> ignore
            if validate problem relocatedSolution then
                if rank problem relocatedSolution > naiveSolutionRank then
                    solution <- relocatedSolution

        with e -> ()

    if validate problem solution then
        solution
    else
        failwith "Solution not valid"

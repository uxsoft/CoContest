module DrykAssociation

open Problem
open System.Collections.Generic
open System

type FacilityDistance = 
    { Facility : int;
      Distance : double }

let rec assign (problem:Problem) (solution:Chromosome) (customer:int) (options : IEnumerator<FacilityDistance>) (remainingCapacity: float []) = 
            
    let canAssign (fd : FacilityDistance) = 
        let demand = problem.Demands.[customer]
        let availableCapacity = remainingCapacity.[fd.Facility]
        demand <= availableCapacity
   
    let optionsHead = options.Current
    if canAssign optionsHead then 
        let demand = problem.Demands.[customer]
        let facility = optionsHead.Facility
        solution.[customer] <- facility
        remainingCapacity.[facility] <- remainingCapacity.[facility] - demand
        true
    else  if options.MoveNext() then 
            assign problem solution customer options remainingCapacity
          else false 

let run (problem : Problem) = 
    let random = new Random()
    //naively assign customers    
    let mutable solution = Array.zeroCreate problem.NumberOfCustomers
    let remainingCapacity = problem.Capacities.Clone() :?> float []
    //
    let homelessCustomers = List<int>()

    for customer in  [0..problem.NumberOfCustomers - 1] |> List.sortByDescending (fun c -> problem.Demands.[c]) do
        let facilities = 
            seq { 
                for facility in 0..problem.NumberOfFacilities - 1 do
                    yield { Distance = problem.Distances.[customer, facility];
                            Facility = facility }
            }
            |> Seq.sortBy (fun fd ->  fd.Distance * random.NextDouble())
            
        let facilitiesEnum = facilities.GetEnumerator()
        if facilitiesEnum.MoveNext() then
            if not <| assign problem solution customer facilitiesEnum remainingCapacity then
                failwith "houston we have homeless"
        else
            failwith "no facilities"    

    let naiveSolutionRank = rank problem solution
    //Redistribute loners to avoid building unnecessary facilities 
    let redistributionThreshold = 0
    let redistributionOptions  = solution
                                   |> Seq.mapi (fun c f -> KeyValuePair(f, c))
                                   |> Seq.groupBy (fun kvp -> kvp.Key)
                                   |> Seq.filter (fun (f, cs) -> cs|> Seq.length > redistributionThreshold)
                                   |> Seq.map (fun (f, cs) -> f)

    let redistributionCandidates = solution
                                   |> Seq.mapi (fun c f -> KeyValuePair(f, c))
                                   |> Seq.groupBy (fun kvp -> kvp.Key)
                                   |> Seq.filter (fun (f,cs) -> cs|> Seq.length <= redistributionThreshold)
                                   |> Seq.sortByDescending (fun (f, cs) -> problem.Costs.[f] * random.NextDouble()) 
                                   |> Seq.map (fun (f, cs) -> cs |> Seq.map (fun kvp -> kvp.Value))

    for facilityCustomers in redistributionCandidates do
        try
            let relocatedSolution = clone solution
            let remainingCapacityAfterRelocation = remainingCapacity.Clone() :?> float[]
            
            for movingCustomer in facilityCustomers do
                let orderedRelocationOptions = redistributionOptions 
                                               |> Seq.map (fun f -> {Facility = f; Distance = problem.Distances.[movingCustomer, f]})
                                               |> Seq.sortBy(fun fd -> fd.Distance * random.NextDouble())
                
                let oRPenum = (orderedRelocationOptions.GetEnumerator())
                if oRPenum.MoveNext() then
                    assign problem relocatedSolution movingCustomer oRPenum remainingCapacityAfterRelocation |> ignore
            if validate problem relocatedSolution then
                if rank problem relocatedSolution > naiveSolutionRank then
                    solution <- relocatedSolution

        with e -> ()

    if validate problem solution then
        Seq.singleton solution
    else
        failwith "Solution not valid"

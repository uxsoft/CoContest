module DistanceMindedBruteForce

open Problem
open System.Linq



let run file =
    let problem = loadProblem (file)

    let customers = Enumerable.Range(0, problem.NumberOfCustomers).ToList()
    for permutation in Combinatorics.Collections.Permutations(customers) do
        ()
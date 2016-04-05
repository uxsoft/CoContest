module Knapsacks

open Problem

let solver = OrToolsMilpManager.Implementation.OrToolsMilpSolver(1000)

let knapsack problem =
    ()

let run file =
    let problem = loadProblem file
    ()
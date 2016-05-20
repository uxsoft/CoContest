module Seq

open System

let sortByProbability (rng : Random) (f : 'a -> double)  (col : 'a seq)= 
    col
    |> Seq.map (fun item -> (item, rng.NextDouble() * f (item)))
    |> Seq.sortBy (fun (item, p) -> p)
    |> Seq.map (fun (item, p) -> item)

module Program

open Problem
open System
open System.Collections.Generic
open System.Drawing
open System.Globalization
open System.IO
open System.Linq
open System.Text
open NaturalSelection



[<EntryPoint>]
let main argv = 
    NaturalSelection.run (Array.head argv)

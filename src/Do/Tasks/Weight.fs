module Tasks.Weight

open System
open NUnit.Framework
open Tasks
open Tasks.Measure

type T =
    | Minimum
    | Weight of double
    | Maximum
let init = Weight 1.
let max = Maximum
let min = Minimum

let sum (weights: T list) =
    if (List.contains max weights) then
        Double.MaxValue
    else
        weights
        |> List.sumBy (fun w ->
            match w with
            | Weight d -> d
            | _ -> 0.
            )

let aggregateByMultiplication (measurements: Measurement list) =
    if List.contains Measure.Force measurements then
        max
    else if List.contains Measure.Suppress measurements then
        min
    else
        measurements
        |> List.map (fun m ->
                match m with
                | Measurement n -> n
                | _ -> 1.
            )
        |> List.fold (*) 1.
        |> Weight

let weight aggregator (measures: Measure.T list) (task: Task.T) =
    measures
    |> List.map (fun m -> m task)
    |> aggregator
    
module Tests =
    open FsUnit
    
    [<TestFixture>]
    type ``Given a series of measures`` () =
        let m m task = m
        let suppressionMeasure = m Measure.Suppress
        let forceMeasure = m Measure.Force
        
        [<Test>]
        member x.``If there are any suppression measures, the end weight is zero`` () =
            weight aggregateByMultiplication [suppressionMeasure] (Task.Tests.newTask)
            |> should equal (Minimum)
            
        [<Test>]
        member x.``If there are any force measures, the end weight is big`` () =
            weight aggregateByMultiplication [forceMeasure] (Task.Tests.newTask)
            |> should equal (Maximum)
            
        [<Test>]
        member x.``If there are both force and supression measures, the end weight is big`` () =
            weight aggregateByMultiplication [forceMeasure; suppressionMeasure] (Task.Tests.newTask)
            |> should equal (Maximum)
            
        [<Test>]
        member x.``If there are only standard measures, the end weight is their product`` () =
            weight aggregateByMultiplication [m (Measurement 1.); m (Measurement 2.)] (Task.Tests.newTask)
            |> should equal (Weight 2.)

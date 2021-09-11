module Tasks.Weight

open System
open NUnit.Framework
open Tasks
open Tasks
open Tasks.Importance
open Tasks.Measure


type T = _T * Measurement list
and _T =
    | Minimum
    | Weight of double
    | Maximum
    
type Aggregator = Measurement list -> T

let result ((r, _): T) = r
let init = Weight 1.
let max = Maximum
let min = Minimum

let create _t measures = (_t, measures)

let stringify (t: T) =
    let _stringify (m: Measurement list) =
        m
        |> List.map Measure.stringify
        |> String.concat "; "
    match t with
    | (r, m) -> sprintf "%A [%s]" r (_stringify m)

let sum (weights: T list) =
    if (List.contains max (weights |> List.map result)) then
        Double.MaxValue
    else
        weights
        |> List.sumBy (fun w ->
            match w with
            | (Weight d, _) -> d
            | _ -> 0.
            )

let aggregateByMultiplication (measurements: Measurement list): T =
    let results = measurements |> List.map Measure.result
    let _aggregate = 
        if List.contains Measure.Force results then
            max
        else if List.contains Measure.Suppress results then
            min
        else if not <| List.exists (fun m -> m <> Measure.Neutral) results then
             min
        else
            results
            |> List.map (fun m ->
                    match m with
                    | Measurement n -> n
                    | _ -> 1.
                )
            |> List.fold (*) 1.
            |> Weight
    (_aggregate, measurements)

let weight (aggregator: Aggregator) (measures: Measure.T list) (task: Task.T) =
    let measurements =
        measures
        |> List.map (fun m -> m task)
        
    aggregator measurements
    
    
module Tests =
    open FsUnit
    
    [<TestFixture>]
    type ``Given a series of measures`` () =
        let m m task = Measure.create m ""
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

module Tasks.Measures.RelevanceMeasure

open System
open Tasks
open Tasks.RelevanceRange

let name = "relevance"

let measure (now: DateTime) (task: Task.T) =
    if (List.isEmpty task.completed) then
        match task.relevanceRange with
        | Always -> Measure.neutral
        | After dt ->
            if dt < now then
                Measure.neutral
            else
                Measure.Suppress
        | Before dt ->
            let range =
                (dt - task.created).Ticks |> double
            let timeRemaining =
                (dt - now).Ticks |> double
            
            Measure.tendTowardsForce range timeRemaining
        | Between (lower, upper) ->
            if (lower <= now) then
                let range =
                    (upper - lower).Ticks |> double
                let timeRemaining =
                    (upper - now).Ticks |> double
                
                Measure.tendTowardsForce range timeRemaining
            else
                Measure.Suppress
    else Measure.neutral
    
module Tests =
    open FsUnit
    open NUnit.Framework
    
    [<TestFixture>]
    type ``Given a relevance range`` () =
        let task = Task.Tests.newTask
        
        let dt y m d = DateTime(y, m, d)
        
        let now = dt 2020 5 5
        
        [<Test>]
        member x.``When there is no set range, the measure is neutral`` () =
            measure now task
            |> should equal Measure.neutral
        
        [<Test>]
        member x.``When we are before the beginning, then the measure is suppressive`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.After (dt 2020 5 6)))
            |> should equal Measure.Suppress
            
        [<Test>]
        member x.``When we are after the beginning, then the measure is neutral`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.After (dt 2020 5 4)))
            |> should equal Measure.neutral
            
        [<Test>]
        member x.``When we are after the deadline, then the measure is forceful`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.Before (dt 2020 5 4)))
            |> should equal Measure.Force
            
        [<Test>]
        member x.``When we are before the deadline, then the measure is not suppressive`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.Before (dt 2020 5 6)))
            |> should equal (Measure.Measurement 1.0)
            
        [<Test>]
        member x.``When we are before the range, then the measure is suppressive`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.Between (dt 2020 5 6, dt 2020 5 10)))
            |> should equal Measure.Suppress
            
        [<Test>]
        member x.``When we are inside the range, then the measure gradually scales up`` () =
            let one =
                measure now (task |> Task.setRelevanceRange (RelevanceRange.Between (dt 2020 5 4, dt 2020 5 10)))
            let two =
                measure (dt 2020 5 8) (task |> Task.setRelevanceRange (RelevanceRange.Between (dt 2020 5 4, dt 2020 5 10)))
                
            two > one
            |> should be True
            
        [<Test>]
        member x.``The scaling up over time should be appropriately gradual over a week`` () =
            let on n =
                measure n (task |> Task.setRelevanceRange (RelevanceRange.Between (dt 2020 5 3, dt 2020 5 10)))
                
            on (dt 2020 5 2) |> should equal Measure.Suppress
            on (dt 2020 5 3) |> should lessThan (Measure.Measurement 1.1)
            on (dt 2020 5 5) |> should greaterThan (Measure.Measurement 2.0)
            on (dt 2020 5 7) |> should greaterThan (Measure.Measurement 4.0)
            on (dt 2020 5 9) |> should greaterThan (Measure.Measurement 6.0)
            on (dt 2020 5 11) |> should equal Measure.Force
            
        [<Test>]
        member x.``When we are after the range, then the measure is forceful`` () =
            measure now (task |> Task.setRelevanceRange (RelevanceRange.Between (dt 2020 5 1, dt 2020 5 5)))
            |> should equal Measure.Force
        
            

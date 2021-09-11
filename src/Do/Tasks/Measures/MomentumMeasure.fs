module Tasks.Measures.MomentumMeasure

open System
open Tasks
open Tasks.Measure

let name = "momentum"

let measure (now: DateTime) (task: Task.T) =
    match task.momentum with
    | Momentum.Momentum n ->
        //y = 100 - (x^1.2) is about right for multiplier? The exponent controls the rate of decay
        let days = (now - task.lastUpdated).TotalDays
        let multiplier =
            let rate = 100.0 - (Math.Pow(days, 1.8))
            if (rate < 0.0) then 0.0
            else rate / 100.0
        
        if n > 1.0 then
            Measure.Measurement (Math.Pow(((1.0 + ((n - 1.0) * multiplier)), 0.7)))
        else 
            Measure.Measurement (1.0 - ((1.0 - n) * multiplier))

module Tests =
    open FsUnit
    open NUnit.Framework
    
    let measured m =
        match m with
        | Measurement n -> n
        | _ -> 0.0
    [<TestFixture>]
    type ``Given a task with a certain momentum`` () =
        [<Test>]
        member x.``When it was set recently it is respected`` () =
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 20.)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 5, 5))
            |> should equal (Measurement (20.))
            
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 0.1)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 5, 5))
            |> measured
            |> should lessThan 0.11
            
            
        [<Test>]
        member x.``When it was set a long time ago it is respected less`` () =
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 20.)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 6, 5))
            |> measured
            |> should lessThan 10.0
            
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 0.1)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 6, 5))
            |> measured
            |> should greaterThan 0.6
            
        [<Test>]
        member x.``When it was set a very long time ago it is respected very little`` () =
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 20.)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 7, 5))
            |> measured
            |> should lessThan 2
            
            Task.Tests.newTask
            |> Task.setMomentum (Momentum.create 0.1)
            |> Task.setLastUpdated (DateTime(2020, 5, 5))
            |> measure (DateTime(2020, 7, 5))
            |> measured
            |> should greaterThan 0.99
            

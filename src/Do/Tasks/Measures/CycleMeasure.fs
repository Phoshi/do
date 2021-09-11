module Tasks.Measures.CycleMeasure
open System
open FsUnit
open Tasks
open CycleRange

let name = "cycle"

let measure (now: DateTime) (task: Task.T) =
    match task.completed with
    | [] -> Measure.neutral
    | completions ->
        let completed =
            completions
            |> List.sortByDescending id
            |> List.head
        match task.cycleRange with
        | Never -> Measure.Suppress
        | After dt ->
            if CycleTime.compare completed now dt = CycleTime.After then
                Measure.neutral
            else
                Measure.Suppress
        | Before dt ->
            let roundedCompleted = CycleTime.clampToPrecision completed dt
            let range =
                (CycleTime.toTimeSpan now dt).Ticks |> double
            let timeRemaining =
                (((CycleTime.add dt roundedCompleted) - now).Ticks |> double)
            
            Measure.tendTowardsForce range timeRemaining
        | Between (lower, upper) ->
            if CycleTime.compare completed now lower = CycleTime.After then
                let roundedLower = CycleTime.clampToPrecision now lower
                let roundedUpper = CycleTime.clampToPrecision now upper
                let roundedCompleted = CycleTime.clampToPrecision completed upper
                let range =
                    ((CycleTime.toTimeSpan roundedLower upper) - (CycleTime.toTimeSpan roundedUpper lower)).Ticks |> double
                let timeRemaining =
                    (((CycleTime.add upper roundedCompleted) - now).Ticks |> double)
                
                Measure.tendTowardsForce range timeRemaining
            else
                Measure.Suppress
    
module Tests =
    open NUnit.Framework
    
    [<TestFixture>]
    type ``Given a cycle range`` () =
        let task = Task.Tests.newTask
        
        let dt y m d = DateTime(y, m, d)
        let dtt y m d h M = DateTime(y, m, d, h, M, 0)
        let ts d = TimeSpan.FromDays(d |> float) |> CycleTime.fromTimeSpan
        
        let now = dt 2020 5 5 //a tuesday!
        
        let measured m =
            match m with
            | Measure.Measurement n -> n
            | _ -> 0.0
        
        [<Test>]
        member x.``'After' ranges use relative-precision values`` () =
            measure now (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (After (ts 1)))
            |> should equal Measure.neutral
            
            measure now (task |> Task.addCompleted (dtt 2020 5 1 12 53) |> Task.setCycleRange (After (CycleTime.parse "1 week")))
            |> should equal Measure.neutral
            
            measure now (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (After (CycleTime.parse "1 week")))
            |> should equal Measure.Suppress
            
            measure now (task |> Task.addCompleted (dtt 2020 5 1 12 53) |> Task.setCycleRange (After (CycleTime.parse "1 month")))
            |> should equal Measure.Suppress
            
            measure now (task |> Task.addCompleted (dtt 2020 4 29 12 53) |> Task.setCycleRange (After (CycleTime.parse "1 month")))
            |> should equal Measure.neutral
            
        [<Test>]
        member x.``'Before' ranges use relative-precision values`` () =
            measure now (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (Before (ts 1)))
            |> should equal Measure.Force
            
            measure (dtt 2020 5 4 18 00) (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (Before (ts 1)))
            |> measured
            |> should be (greaterThan 1.0)
            
            measure now (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (Before (CycleTime.parse "1 month")))
            |> measured
            |> should be (greaterThan 1.0)
            
            measure (dt 2020 6 1) (task |> Task.addCompleted (dtt 2020 5 4 12 53) |> Task.setCycleRange (Before (CycleTime.parse "1 month")))
            |> should equal Measure.Force
            
        [<Test>]
        member x.``'Between' ranges use relative-precision values`` () =
            measure (dtt 2020 5 3 18 00) (task |> Task.addCompleted (dtt 2020 5 3 12 53) |> Task.setCycleRange (Between (ts 1, ts 2)))
            |> should equal Measure.Suppress
            
            measure (dtt 2020 5 4 18 00) (task |> Task.addCompleted (dtt 2020 5 3 12 53) |> Task.setCycleRange (Between (ts 1, ts 2)))
            |> measured
            |> should be (greaterThan 1.0)
            
            measure (dt 2020 5 5) (task |> Task.addCompleted (dtt 2020 5 3 12 53) |> Task.setCycleRange (Between (ts 1, ts 2)))
            |> should equal Measure.Force
            
        [<Test>]
        member x.``When there is no set range, the measure is neutral`` () =
            measure now task
            |> should equal Measure.neutral
            
        
        [<Test>]
        member x.``When the task is incomplete, the measure is neutral`` () =
            measure now (task |> Task.setCycleRange (After (ts 5)))
            |> should equal Measure.neutral
            
        [<Test>]
        member x.``When the task was completed too recently, the measure is suppressive`` () =
            measure now (task |> Task.setCycleRange (After (ts 5)) |> Task.setCompleted [dt 2020 5 3])
            |> should equal Measure.Suppress
            
        [<Test>]
        member x.``When the task is after the deadline, the measure is forceful`` () =
            measure now (task |> Task.setCycleRange (Before (ts 5)) |> Task.setCompleted [dt 2020 4 27])
            |> should equal Measure.Force
        
        
        [<Test>]
        member x.``When the task was completed longer ago than the minimum cycle time, the measure is neutral`` () =
            measure now (task |> Task.setCycleRange (After (ts 5)) |> Task.setCompleted [dt 2020 4 3])
            |> should equal Measure.neutral
            
        [<Test>]
        member x.``After a task with a maximum cycle time is completed, the measure scales up over time`` () =
            let one = measure now (task |> Task.setCycleRange (Before (ts 10)) |> Task.setCompleted [dt 2020 5 1])
            let two = measure now (task |> Task.setCycleRange (Before (ts 10)) |> Task.setCompleted [dt 2020 5 4])
            
            (two < one)
            |> should be True
            
        [<Test>]
        member x.``After a task with a cycle range is completed, the measure is suppressive until the range begins`` () =
            measure now (task |> Task.setCycleRange (Between (ts 5, ts 10)) |> Task.setCompleted [dt 2020 5 3])
            |> should equal Measure.Suppress
            
        [<Test>]
        member x.``After a task with a cycle range is completed, the measure is forceful once the range ends`` () =
            measure now (task |> Task.setCycleRange (Between (ts 1, ts 4)) |> Task.setCompleted [dt 2020 5 1])
            |> should equal Measure.Force
            
        [<Test>]
        member x.``After a task with a cycle range is completed, the measure scales up while we move through the range`` () =
            let one =
                measure now (task |> Task.setCycleRange (Between (ts 1, ts 10)) |> Task.setCompleted [dt 2020 5 1])
            let two =
                measure (dt 2020 5 8) (task |> Task.setCycleRange (Between (ts 1, ts 10)) |> Task.setCompleted [dt 2020 5 1])
                
            (two > one)
            |> should be True
            
        [<Test>]
        member x.``After a task with a cycle range is completed, the measure scales up in a reasonable amount`` () =
            let task = (task |> Task.setCycleRange (Between (CycleTime.parse "1 month", CycleTime.parse "2 months")) |> Task.setCompleted [dt 2020 4 15])
            let one =
                measure (dt 2020 4 27) task
            let two =
                measure (dt 2020 5 1) task
            let three =
                measure (dt 2020 5 10) task
            let four =
                measure (dt 2020 5 20) task
            let five =
                measure (dt 2020 5 26) task
            let six =
                measure (dt 2020 5 28) task
            let seven =
                measure (dt 2020 6 1) task
                
            one |> should equal Measure.Suppress
            seven |> should equal Measure.Force
            
            [two; three; four; five; six] |> List.map measured |> should be ascending
            
            two |> measured |> should be (lessThan 1.5)
            six |> measured |> should be (greaterThan 10)
        
            

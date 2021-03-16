module Tasks.Measures.CycleMeasure
open System
open FsUnit
open Tasks
open CycleRange

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
            if (CycleTime.add dt completed) < now then
                Measure.neutral
            else
                Measure.Suppress
        | Before dt ->
            let range =
                (CycleTime.toTimeSpan now dt).Ticks |> double
            let timeRemaining =
                ((((CycleTime.add dt completed) - now).Ticks |> double))
            
            Measure.tendTowardsForce range timeRemaining
        | Between (lower, upper) ->
            if (CycleTime.add lower completed < now) then
                let range =
                    ((CycleTime.toTimeSpan now upper) - (CycleTime.toTimeSpan now lower)).Ticks |> double
                let timeRemaining =
                    (((CycleTime.add upper completed) - now).Ticks |> double)
                
                Measure.tendTowardsForce range timeRemaining
            else
                Measure.Suppress
    
module Tests =
    open FsUnit
    open NUnit.Framework
    
    [<TestFixture>]
    type ``Given a cycle range`` () =
        let task = Task.Tests.newTask
        
        let dt y m d = DateTime(y, m, d)
        let ts d = TimeSpan.FromDays(d |> float) |> CycleTime.fromTimeSpan
        
        let now = dt 2020 5 5
        
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
            
        
            

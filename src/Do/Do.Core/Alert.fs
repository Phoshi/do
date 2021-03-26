module Do.Core.Alert

open System
open System.Threading.Tasks
open Tasks
open Tasks.CycleRange

type T =
    | None
    | ReviewRequested
    | InformationRequired
    | TaskAssigned
    | TaskRequired
    
let alertLevel (measure: Task.T -> Weight.T) (now: DateTime) (iteration: CycleRange.CycleTime.T) (confidenceLevels: Confidence.T list) (tasks: Task.T list) =
    let hasLowConfidenceTasks =
        confidenceLevels
        |> List.collect (LowConfidenceSelection.findLowConfidenceTasks tasks)
        |> List.isEmpty
        |> not
        
    let hasAssignedTasks =
        LowConfidenceSelection.findHighConfidenceTasks tasks (Confidence.zero "assigned")
        |> List.isEmpty
        |> not
        
    let lastCompletedTask =
        tasks
        |> List.collect (fun t -> t.completed)
        |> List.sortDescending
        |> List.tryHead
        
    let haveLapsedReviewTime =
        lastCompletedTask
        |> Option.map (fun c -> CycleTime.compare c now iteration)
        |> Option.contains CycleTime.After
        
    let hasForcedTask =
        tasks
        |> List.exists (fun t -> measure t = Weight.Maximum)
        
    if hasAssignedTasks then
        TaskAssigned
    else if hasLowConfidenceTasks then
        InformationRequired
    else if haveLapsedReviewTime then
        ReviewRequested
    else
       None
    
module Tests =
    open FsUnit
    open NUnit.Framework
    
    let newTask = Task.Tests.newTask
    let task completed =
        newTask
        |> Task.addCompleted completed
        |> Task.setConfidence (Confidence.full "conf1")
        |> Task.setConfidence (Confidence.full "conf2")
        
    let assignedTask =
        newTask
        |> Task.setConfidence (Confidence.full "assigned")
    
    [<TestFixture>]
    type ``Given a collection of tasks`` () =
        let now = DateTime(2020, 5, 5, 12, 00, 00)
        let level = alertLevel (fun _ -> Weight.init) now (CycleTime.parse "1 day") [Confidence.full "conf1"; Confidence.full "conf2"]
        
        
        [<Test>]
        member this.``When there are no tasks, there is no alert`` () =
            level []
            |> should equal None
            
        [<Test>]
        member this.``When we have completed a task today and nothing needs doing, there is no alert`` () =
            level [task (now.Date)]
            |> should equal None
            
        [<Test>]
        member this.``When there is a task we are not confident on yet, the alert is Information Required`` () =
            level [newTask; task (now.Date)]
            |> should equal InformationRequired
       
        [<Test>]
        member this.``When we have an assigned task, the alert is Task Assigned`` () =
            level [assignedTask]
            |> should equal TaskAssigned
            
        [<Test>]
        member this.``When we haven't done a review in our task cycle time, the alert is Review Requested`` () =
            level [task (now.Date.AddDays(-1.0))]
            |> should equal ReviewRequested
            
        [<Test>]
        member this.``When a task is measured forcefully, the alert is Task Required`` () =
            alertLevel (fun _ -> Weight.Maximum) now (CycleTime.parse "1 day") [Confidence.full "conf1"; Confidence.full "conf2"] [newTask]
            |> should equal TaskRequired

module Do.Core.Alert

open System
open System.Threading.Tasks
open MarkdownSource
open Tasks
open Tasks.CycleRange

type T =
    | None
    | ReviewRequested
    | InformationRequired
    | TaskAssigned
    | TaskRequired
    
let alertLevel (measure: Task.T -> Weight.T) (now: DateTime) (iterationLapsed: Task.T list -> bool) (isConfident: Task.T -> bool) (tasks: Task.T list) =
    let hasLowConfidenceTasks =
        tasks
        |> List.where (fun t -> not <| isConfident t)
        |> List.isEmpty
        |> not
        
    let hasAssignedTasks =
        LowConfidenceSelection.findHighConfidenceTasks tasks (Confidence.zero "assigned")
        |> List.isEmpty
        |> not
        
    let haveLapsedReviewTime =
        iterationLapsed tasks
        
        
    let forcedTasks =
        tasks
        |> List.where (fun t -> measure t |> Weight.result = Weight.Maximum)
        
    let hasForcedTask =
        forcedTasks
        |> List.isEmpty
        |> not
        
    if hasForcedTask then
        TaskRequired
    else if hasAssignedTasks then
        TaskAssigned
    else if hasLowConfidenceTasks then
        InformationRequired
    else if haveLapsedReviewTime then
        ReviewRequested
    else
       None
       
       
        
let hasReviewLapsed now (weight: Task.T -> Weight.T) (tagDefinitions: (string * ReviewMeta) list) (baseDefinition: ReviewMeta) (tasks: Task.T list) =
    let anyTagLapsed =
        tagDefinitions
        |> List.exists (fun (t, d) -> ReviewSelection.tagLapsed (ReviewSelection.lapsed now weight d.frequency) t tasks)
        
    let baseLapsed =
        ReviewSelection.untaggedLapsed (ReviewSelection.lapsed now weight baseDefinition.frequency) (tagDefinitions |> List.map fst) tasks
        
    anyTagLapsed || baseLapsed
        
    
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
        
    let tagged tag task = Task.setTags [tag] task
        
    [<TestFixture>]
    type ``Given tasks which may have lapsed review periods`` () =
        let now = DateTime(2020, 5, 5, 12, 00, 00)
        let twoDaysAgo = now.AddDays -2.0
        let r p =
            let meta = ReviewMeta()
            meta.assignments <- 1
            meta.size <- 1
            meta.frequency <- p
            meta
            
        let scale _ = Weight.create Weight.init []
            
        [<Test>]
        member this.``If no review periods are supplied then no periods have lapsed`` () =
            hasReviewLapsed now scale [] (r "") [newTask; newTask]
            |> should equal false
            
        [<Test>]
        member this.``If the base period has not expired and there are no overrides, then no periods have lapsed`` () =
            hasReviewLapsed now scale [] (r "1 day") [task now; task now]
            |> should equal false
            
        [<Test>]
        member this.``If the base period has not expired and nor have overrides, then no periods have lapsed`` () =
            hasReviewLapsed now scale [("test", r "5 days")] (r "1 day") [task now; task now]
            |> should equal false
            
        [<Test>]
        member this.``If the base period has expired but overrides have not, and no tasks fall to the default, then no periods have lapsed`` () =
            hasReviewLapsed now scale [("test", r "5 days")] (r "1 day") [task twoDaysAgo |> tagged "test"; task twoDaysAgo |> tagged "test"]
            |> should equal false
            
        [<Test>]
        member this.``If the base period has expired and no tasks fall under the overrides, then the period has lapsed`` () =
            hasReviewLapsed now scale [("test", r "5 days")] (r "1 day") [task twoDaysAgo; task twoDaysAgo]
            |> should equal true
            
        [<Test>]
        member this.``If the override period has not expired but tasks do not fall under the overrides, then the period has not lapsed`` () =
            hasReviewLapsed now scale [("test", r "1 day")] (r "10 days") [task twoDaysAgo; task twoDaysAgo]
            |> should equal false
            
        [<Test>]
        member this.``If the override period has expired and tasks fall under the overrides, then the period has lapsed`` () =
            hasReviewLapsed now scale [("test", r "1 day")] (r "10 days") [task twoDaysAgo |> tagged "test"; task twoDaysAgo]
            |> should equal true
         
    
    [<TestFixture>]
    type ``Given a collection of tasks`` () =
        let now = DateTime(2020, 5, 5, 12, 00, 00)
        let level = alertLevel (fun _ -> Weight.create Weight.init []) now (fun _ -> false) (LowConfidenceSelection.higherConfidenceThan (Confidence.zero "conf1"))
        
        
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
            alertLevel (fun _ -> Weight.create Weight.init []) now (fun _ -> true) (LowConfidenceSelection.higherConfidenceThan (Confidence.zero "conf1")) [task (now.Date.AddDays(-1.0))]
            |> should equal ReviewRequested
            
        [<Test>]
        member this.``When a task is measured forcefully, the alert is Task Required`` () =
            alertLevel (fun _ -> Weight.create Weight.Maximum []) now (fun _ -> false) (fun _ -> true) [newTask]
            |> should equal TaskRequired

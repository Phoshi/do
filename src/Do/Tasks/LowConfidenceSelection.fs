module Tasks.LowConfidenceSelection

open System

let lowerConfidenceThan (confidence: Confidence.T) (task: Task.T) =
    task.confidence
    |> List.tryFind (fun c -> c.measure = confidence.measure)
    |> Option.map (fun c -> c.confidence < confidence.confidence)
    |> Option.defaultValue true
    
let findLowConfidenceTasks (tasks: Task.T list) (minimumConfidence: Confidence.T) =
    tasks
    |> List.where (lowerConfidenceThan minimumConfidence)
    
let higherConfidenceThan (confidence: Confidence.T) (task: Task.T) =
    task.confidence
    |> List.tryFind (fun c -> c.measure = confidence.measure)
    |> Option.map (fun c -> c.confidence > confidence.confidence)
    |> Option.defaultValue false
    
let findHighConfidenceTasks (tasks: Task.T list) (minimumConfidence: Confidence.T) =
    tasks
    |> List.where (higherConfidenceThan minimumConfidence)
    
let findComparisonContender (allTasks: Task.T list) (task: Task.T) (measure: Task.T -> double) (lower: double option) (upper: double option) =
    let isInBounds (t: Task.T) =
        let m = measure t
        
        let lowerGood =
            lower
            |> Option.map ((>)m)
            |> Option.defaultValue true
            
        let upperGood =
            upper
            |> Option.map ((<)m)
            |> Option.defaultValue true
        
        lowerGood && upperGood && (t.title <> task.title)
        
    if Option.isNone lower then
        allTasks
        |> List.where isInBounds
        |> List.sortBy measure
        |> List.tryHead
    else if Option.isNone upper then
        allTasks
        |> List.where isInBounds
        |> List.sortByDescending measure
        |> List.tryHead
    else
        let goal =
            lower
            |> Option.bind
                   (fun l ->
                    Option.map
                        (fun u -> (l + u) / 2.0)
                        upper)
            |> Option.get
                   
        allTasks
        |> List.where isInBounds
        |> List.sortBy (fun t ->
            ((measure t) - goal) |> Math.Abs
            )
        |> List.tryHead
    
module Tests =
    open FsUnit
    open NUnit.Framework
    
    let t urgency =
        Task.Tests.newTask |> Task.setUrgency (Urgency.create urgency) |> Task.setTitle ("Task with urgency " + urgency.ToString())
        
    let newTask = t 1.0
    
    let library = [
        t 0.25
        t 0.55
        t 0.85
        t 1.25
        t 1.55
        t 1.65
        t 1.70
        t 1.80
        t 2.00
        t 5.00
    ] 
    
    let urgency (t: Task.T) = Urgency.value t.urgency
    
    [<TestFixture>]
    type ``Given a low confidence task`` () =
        [<Test>]
        member x.``There are no comparators if it is the only task`` () =
            findComparisonContender [] newTask urgency None None
            |> should equal None
            
        [<Test>]
        member x.``If we don't know the confidence lower bound, then we pick the lowest item`` () =
            findComparisonContender library newTask urgency None None
            |> should equal (Some (t 0.25))
            
        [<Test>]
        member x.``If we know the lower, but not upper, bound, then we pick the highest item`` () =
            findComparisonContender library newTask urgency (Some 0.25) None
            |> should equal (Some (t 5.00))
            
        [<Test>]
        member x.``If we know both bounds, then we take the item closest to being in the middle`` () =
            findComparisonContender library newTask urgency (Some 0.25) (Some 5.00)
            |> should equal (Some (t 2.00))
            
        [<Test>]
        member x.``If we know both bounds, but there are no tasks within those bounds, then we have nothing`` () =
            findComparisonContender library newTask urgency (Some 2.00) (Some 5.00)
            |> should equal None

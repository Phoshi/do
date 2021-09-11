module Do.Core.ReviewSelection

open System
open Tasks
open Tasks.CycleRange

let _review (rng: Random) measure n (tasks: Task.T list) =
    
    Selector.select
        measure
        (rng.Next())
        n
        tasks
        
let review measure (categories: (int * (Task.T -> bool)) list) (tasks: Task.T list) =
    let rng = Random()
    let mutable overallReview = []
    let remainingTasks () =
        tasks |> List.where (fun t -> List.contains t overallReview |> not)
        
    for (n, pred) in categories do 
        let specificEntries =
            _review rng measure n (remainingTasks () |> List.where pred)
        overallReview <- specificEntries @ overallReview
        
    overallReview
        
        
let selectedTasks measure (categories: (int * (Task.T -> bool)) list) (options: Task.T seq) =
    review
        measure
        categories
        (options |> List.ofSeq)
    
let taskWeight measure task =
    measure task
    

let _anyTasksActive (weight: Task.T -> Weight.T) (tasks: Task.T list) =
    tasks
    |> List.map weight
    |> List.exists (fun w -> match w |> Weight.result with | Weight.Minimum -> false | _ -> true)
    
let _hasTaskLapsed now task range =
    if Option.isNone task then
        false
    else
        task
        |> Option.bind Task.lastCompleted
        |> Option.map (fun t -> CycleTime.compare t now range)
        |> Option.map (fun ct -> ct = CycleTime.After)
        |> Option.defaultValue true

let lapsed now weight frequency (tasks: Task.T list)  =
    let availableTasks = 
        tasks
        |> List.sortByDescending (Task.lastCompleted)
        
    let anyAssigned =
        tasks
        |> List.exists (fun t -> Confidence.confidenceLevel "assigned" t.confidence > 0.0)
        
    let latestTask =
        availableTasks
        |> List.tryHead
        
    if frequency = "" then
        false
    else if anyAssigned then
        false
    else
        let cycleTime = CycleTime.parse frequency
            
        _hasTaskLapsed now latestTask cycleTime && (_anyTasksActive weight availableTasks)

let tagLapsed lapsed tag (tasks: Task.T list) =
    let availableTasks = 
        tasks
        |> List.where (Task.isTagged tag)
        
    lapsed availableTasks
    
    
let untaggedLapsed lapsed tags (tasks) =
        
    let availableTasks =
        tasks
        |> List.where (Task.notTagged tags)
        |> List.sortByDescending Task.lastCompleted
        
    lapsed availableTasks

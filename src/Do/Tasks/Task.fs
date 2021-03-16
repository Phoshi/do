module Tasks.Task

open System

type T = {
    filepath: string
    
    title: string
    description: string
    tags: Tags.T
    created: DateTime
    
    importance: Importance.T
    urgency: Urgency.T
    
    relevanceRange: RelevanceRange.T
    cycleRange: CycleRange.T
    
    completed: DateTime list
    lastUpdated: DateTime
    momentum: Momentum.T
    
    confidence: Confidence.T list
}

let create filepath title description tags created importance urgency relevance cycle completed lastUpdated momentum confidence =
    {
        filepath = filepath
        title = title
        description = description
        tags = tags
        created = created
        importance = importance
        urgency = urgency
        relevanceRange = relevance
        cycleRange = cycle
        completed = completed
        lastUpdated = lastUpdated
        momentum = momentum
        confidence = confidence
    }
    
let createSimple filepath title description created =
    create
        filepath
        title
        description
        []
        created
        Importance.init
        Urgency.init
        RelevanceRange.Always
        CycleRange.Never
        []
        created
        Momentum.init
        []
    
let setTitle title task =
    {task with title = title}

let setDescription desc task =
    {task with description = desc}
    
let setMomentum m task =
    {task with momentum = m}
    
let setRelevanceRange range task =
    {task with relevanceRange = range}
    
let setCycleRange range task =
    {task with cycleRange = range}
    
let setCreated created task =
    {task with created = created}
    
let setLastUpdated lastUpdated task =
    {task with lastUpdated = lastUpdated}
    
let setCompleted completed task =
    {task with completed = completed}
    
let addCompleted completion task =
    { task with completed = completion :: task.completed }
    
let setConfidence conf task =
    let _with (prior: Confidence.T list) =
        conf :: (prior |> List.where (fun c -> c.measure <> conf.measure))
        
    { task with confidence = _with task.confidence }
    
let setUrgency u t =
    {t with urgency = u}
    
let setImportance i t =
    {t with importance = i}
    
module Tests =
    let newTask = create "file.md" "Test task" "Test task desc" [] (DateTime(2020, 5, 5)) Importance.init Urgency.init RelevanceRange.Always CycleRange.Never [] (DateTime(2020, 5, 5)) Momentum.init []
    
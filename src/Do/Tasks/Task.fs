module Tasks.Task

open System
open System.IO
open System.Text.RegularExpressions
open Tasks
open Tasks

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
    
let lastCompleted (task: T) =
    task.completed
    |> List.sortDescending
    |> List.tryHead
    
    
let setConfidence conf task =
    let _with (prior: Confidence.T list) =
        conf :: (prior |> List.where (fun c -> c.measure <> conf.measure))
        
    { task with confidence = _with task.confidence }
    
let setTags tags task =
    { task with tags = tags }
    
let isTagged tag (task: T) =
    task.tags
    |> List.contains tag
    
let isTaggedAll tags (task: T) =
    tags |> List.forall (fun t -> isTagged t task)
    && task.tags.Length = tags.Length
    
let notTagged tags (task: T) =
    tags |> List.exists (fun t -> task.tags |> List.contains t) |> not
    
let setUrgency u t =
    {t with urgency = u}
    
let setImportance i t =
    {t with importance = i}
    
let active (task: T) =
    task.completed.Length = 0
    || task.cycleRange <> CycleRange.Never
    
let parse (now: DateTime) (def: string) =
   let extractTags text =
       Regex.Matches(text, "#([^\s]+)")
       |> Seq.cast<Match>
       |> Seq.map (fun m -> m.Groups.[1].Value)
       |> List.ofSeq
       
   let sanitise (path: string) =
       let invalid = System.IO.Path.GetInvalidFileNameChars()
       String.Join("_", path.Split(invalid, StringSplitOptions.RemoveEmptyEntries))
       
   use reader = new StringReader(def)
   let title = reader.ReadLine()
   let desc = reader.ReadToEnd().Trim()
   let tags = extractTags desc
   
   createSimple (sanitise title + ".md") title desc now
   |> setTags tags
    
module Tests =
    let newTask = create "file.md" "Test task" "Test task desc" [] (DateTime(2020, 5, 5)) Importance.init Urgency.init RelevanceRange.Always CycleRange.Never [] (DateTime(2020, 5, 5)) Momentum.init []
    
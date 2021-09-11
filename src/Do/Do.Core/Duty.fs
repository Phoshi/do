module rec Duties.Duty

open System
open System.Collections.Generic
open System.IO
open Do.Core
open MarkdownSource
open Optional
open Tasks
open Tasks.Measures

type T(root: string) =
    let _tasks () =
        dutyTasks
            (fun p t c contents -> MarkdownFileLoader().Task(root, p, t, c, contents))
            root
        |> Seq.ofList
    
    let mutable _meta =
        dutyMeta root
        
    member this.tasks: Task.T seq = _tasks ()
            
    member this.dutyRoot: string = root
    member this.dutyMeta: Metadata = _meta
    member this.dutyName: string = this.dutyMeta.name
    
    member this.strings = this.dutyMeta.strings
    
    member this.tagSpecificReviews =    
        let tags = this.dutyMeta.tags
        tags.Keys
        |> Seq.map (fun t -> (t, tags.[t].review))
        |> Seq.where (fun (t, r) -> r <> null)
        |> List.ofSeq
    
    member this.tagSpecificMeasures =
        let tags = this.dutyMeta.tags
        tags.Keys
        |> Seq.map (fun t ->
            (t,
             tags.[t].measures |> List.ofSeq
             |> List.append (if tags.[t].``override`` then [] else List.ofSeq this.dutyMeta.measures)
             |> List.distinct
             )
            )
        |> List.ofSeq
        
    member this.reviewDefinition: (int * (Task.T -> bool)) list =
        let tags = this.dutyMeta.tags
        
        let tagDefs =
            tags.Keys
            |> Seq.map (fun t ->
                let review = tags.[t].review
                let isInTag = Task.isTagged t
                if ReviewSelection.tagLapsed (ReviewSelection.lapsed DateTime.Now (this.scales()) review.frequency) t (this.tasks |> List.ofSeq) then
                    Some (review.size, isInTag)
                else
                    Some (0, isInTag) 
            )
            |> List.ofSeq
            
        let baseDef =
            if ReviewSelection.untaggedLapsed (ReviewSelection.lapsed DateTime.Now (this.scales()) this.dutyMeta.review.frequency) (tags.Keys |> List.ofSeq) (this.tasks |> List.ofSeq) then
                Some (this.dutyMeta.review.size, Task.notTagged (tags.Keys |> List.ofSeq))
            else
                Some (0, Task.notTagged (tags.Keys |> List.ofSeq))
                
        baseDef :: tagDefs
        |> List.collect Option.toList
        
    member this.assignmentDefinition: (int * (Task.T -> bool)) list =
        let tags = this.dutyMeta.tags
        
        let tagDefs =
            tags.Keys
            |> Seq.map (fun t ->
                let review = tags.[t].review
                let isInTag = Task.isTagged t
                if ReviewSelection.tagLapsed (ReviewSelection.lapsed DateTime.Now (this.scales()) review.frequency) t (this.tasks |> List.ofSeq) then
                    Some (review.assignments, isInTag)
                else
                    Some (0, isInTag) 
            )
            |> List.ofSeq
            
        let baseDef =
            if ReviewSelection.untaggedLapsed (ReviewSelection.lapsed DateTime.Now (this.scales()) this.dutyMeta.review.frequency) (tags.Keys |> List.ofSeq) (this.tasks |> List.ofSeq) then
                Some (this.dutyMeta.review.assignments, Task.notTagged (tags.Keys |> List.ofSeq))
            else
                Some (0, Task.notTagged (tags.Keys |> List.ofSeq))
                
        baseDef :: tagDefs
        |> List.collect Option.toList
        
        
    member this.scales () =
        let now = DateTime.Now
        let defaultMeasures = (this.dutyMeta.measures |> List.ofSeq)
        
        let scales = 
            Scale.tagAwareScales
                (MeasureParser.parse now)
                (Weight.aggregateByMultiplication)
                defaultMeasures
                (this.tagSpecificMeasures)
        Scale.weigh scales
    
    member this.api: API =
        apiFor this
    
type API =
    abstract update : Task.T * DateTime -> unit
    abstract complete: Task.T * DateTime -> unit
    abstract weight: Task.T * DateTime -> Weight.T
    abstract lowConfidence: Confidence.T -> Task.T seq
    abstract highConfidence: Confidence.T -> Task.T seq
    abstract review: unit -> Task.T seq
    abstract assignedTasks: Task.T seq -> Task.T seq
    abstract alertState: DateTime -> Alert.T
    abstract comparisonValue: Task.T * string -> double
    abstract comparisonOperand: Task.T * string * Nullable<double> * Nullable<double> -> Option<Task.T>
    
    
let dutyMeta root =
    let meta = File.ReadAllText(root + "\\.do\\metadata")
    
    MetadataSource().Metadata(meta)
    
    
let rec dutyTasks loader root =
    let _load (path: string) =
        let title = Path.GetFileNameWithoutExtension path
        let created = File.GetCreationTime path
        let contents = File.ReadAllText path
        
        loader path title created contents
    let directories = Directory.GetDirectories root |> List.ofArray
    let files =
        Directory.GetFiles root
        |> List.ofArray
        |> List.where (fun f -> (Path.GetExtension f) = ".md")
    
    (files |> List.map _load) @ (directories |> List.collect (dutyTasks loader))
    
let apiFor (duty: T) =
    let write(t, d) =
            let task =
                t
                |> Task.setLastUpdated d
                |> Task.setConfidence (Confidence.full "lastUpdated")
            
            let md = MarkdownFileWriter().Write(task)
            let path = Path.Join(duty.dutyRoot, task.filepath)
            File.WriteAllText(path, md)
            
    let confidenceTypeApplies confidence task =
        let measures = Scale.measuresForTask (duty.dutyMeta.measures |> List.ofSeq) (duty.tagSpecificMeasures) task
        List.contains confidence ("assigned"::measures)
            
    {new API with
        member this.update(t, d) = write(t, d)
        
        member this.complete(t, d) = 
            let task = Task.addCompleted d t
            write(task, d)
            
        member this.weight(t, d) =
            ReviewSelection.taskWeight
                (duty.scales ())
                t
                
        member this.lowConfidence(c) = 
            LowConfidenceSelection.findLowConfidenceTasks
                (duty.tasks
                 |> Seq.where (confidenceTypeApplies c.measure)
                 |> List.ofSeq)
                c
            |> Seq.ofList
            
        member this.highConfidence(c) = 
            LowConfidenceSelection.findHighConfidenceTasks
                (duty.tasks
                 |> Seq.where (confidenceTypeApplies c.measure)
                 |> List.ofSeq)
                c
            |> Seq.ofList
            
        member this.review() =
            ReviewSelection.review
                (duty.scales ())
                (duty.reviewDefinition) 
                (duty.tasks |> List.ofSeq)
            |> Seq.ofList
            
        member this.assignedTasks(tasks) =
            ReviewSelection.review
                (duty.scales ())
                (duty.assignmentDefinition)
                (tasks |> List.ofSeq)
            |> Seq.ofList
            
        member this.alertState(now) =
            Alert.alertLevel
                (duty.scales ())
                now
                (Alert.hasReviewLapsed
                     DateTime.Now
                     (duty.scales ())
                     duty.tagSpecificReviews
                     duty.dutyMeta.review)
                (fun t ->
                    [RelevanceMeasure.name; ImportanceMeasure.name; UrgencyMeasure.name]
                    |> List.map (fun m -> confidenceTypeApplies m t && LowConfidenceSelection.lowerConfidenceThan (Confidence.full m) t)
                    |> List.forall not
                    )
                (duty.tasks |> List.ofSeq)
            
        member this.comparisonValue(t, measure) =
            match measure with
            | u when u = UrgencyMeasure.name -> Urgency.value t.urgency
            | i when i = ImportanceMeasure.name -> Importance.value t.importance
            | _ -> failwith "Unknown measure type"
            
        member this.comparisonOperand(task, measure, lower, upper) =
            let maybe (d: Nullable<double>) =
                if d.HasValue
                then Some d.Value
                else None
            let option (m: Task.T option) =
                m
                |> Option.map Option.Some<Task.T>
                |> Option.defaultValue (Option.None<Task.T>())
            let isConfident (t: Task.T) =
                Confidence.confidenceLevel measure (t.confidence) >= 1.0
            let valueOf = (fun t -> this.comparisonValue(t, measure))
                
            let tasks = 
                duty.tasks
                |> Seq.where isConfident
                |> Seq.where (Task.isTaggedAll task.tags)
                |> Seq.where Task.active
                |> List.ofSeq
                
            let lowest =
                tasks
                |> Seq.sortBy valueOf
                |> Seq.tryHead
                |> Option.map valueOf
                
            let highest =
                tasks
                |> Seq.sortByDescending valueOf
                |> Seq.tryHead
                |> Option.map valueOf
                
            LowConfidenceSelection.findComparisonContender
                tasks
                task
                valueOf
                (maybe lower |> Option.orElse lowest)
                (maybe upper |> Option.orElse highest)
            |> option
    }

let create root =
    T(root)

module rec Duties.Duty

open System
open System.IO
open Do.Core
open MarkdownSource
open Optional
open Tasks
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

type T(root: string) = 
    member this.tasks: Task.T seq =
        dutyTasks
            (fun p t c contents -> MarkdownFileLoader().Task(p, t, c, contents))
            root
        |> Seq.ofList
            
    member this.dutyRoot: string = root
    member this.dutyMeta: Metadata = dutyMeta root
    member this.dutyName: string = this.dutyMeta.name
    member this.api: API =
        apiFor this
    
type API =
    abstract update : Task.T * DateTime -> unit
    abstract complete: Task.T * DateTime -> unit
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
            File.WriteAllText(task.filepath, md)
            
    {new API with
        member this.update(t, d) = write(t, d)
        member this.complete(t, d) = 
            let task = Task.addCompleted d t
            write(task, d)
        member this.lowConfidence(c) = 
            LowConfidenceSelection.findLowConfidenceTasks (duty.tasks |> List.ofSeq) c
            |> Seq.ofList
        member this.highConfidence(c) = 
            LowConfidenceSelection.findHighConfidenceTasks (duty.tasks |> List.ofSeq) c
            |> Seq.ofList
        member this.review() =
            ReviewSelection.review (duty.dutyMeta.review.size) (duty.tasks |> List.ofSeq)
            |> Seq.ofList
        member this.assignedTasks(tasks) =
            ReviewSelection.review (duty.dutyMeta.review.assignments) (tasks |> List.ofSeq)
            |> Seq.ofList
        member this.alertState(now) =
            Alert.alertLevel (ReviewSelection.measure ()) now (CycleRange.CycleTime.parse "1 day") [Confidence.full "relevance"; Confidence.full "importance"; Confidence.full "urgency"] (duty.tasks |> List.ofSeq)
        member this.comparisonValue(t, measure) =
            match measure with
            | "urgency" -> Urgency.value t.urgency
            | "importance" -> Importance.value t.importance
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
                
            LowConfidenceSelection.findComparisonContender
                (duty.tasks |> Seq.where isConfident |> List.ofSeq)
                task
                (fun t -> (this.comparisonValue(t, measure)))
                (maybe lower)
                (maybe upper)
            |> option
    }

let create root =
    T(root)

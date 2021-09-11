module Tasks.Scale

type Scales = Task.T -> Measure.T list * Weight.Aggregator

let weigh (scales: Scales) (task: Task.T) =
    let (measure, aggregator) = scales task
    Weight.weight aggregator measure task
    
let measuresForTask defaults (tagSets: (string * string list) list) (task: Task.T) =
    let tagSpecificMeasures =
        tagSets
        |> List.where (fun (t, _) -> task.tags |> List.contains t)
        |> List.collect (fun (_, m) -> m)
        |> List.distinct
        
    if tagSpecificMeasures |> List.isEmpty
    then defaults
    else tagSpecificMeasures
    
    
let tagAwareScales parse aggregator (defaults) (tagSets: (string * string list) list) (task: Task.T) =
    let applicableMeasures = measuresForTask defaults tagSets task
        
    (applicableMeasures |> List.map parse, aggregator)
    
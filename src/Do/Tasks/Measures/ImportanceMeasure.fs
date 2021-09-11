module Tasks.Measures.ImportanceMeasure

open Tasks

let name = "importance"

let measure (task: Task.T) =
    match task.importance with
    | Importance.Importance n -> Measure.Measurement (n * n)
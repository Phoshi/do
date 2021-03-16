module Tasks.Measures.ImportanceMeasure

open Tasks

let measure (task: Task.T) =
    match task.importance with
    | Importance.Importance n -> Measure.Measurement (n * n)
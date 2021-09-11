module Tasks.Measures.UrgencyMeasure

open Tasks

let name = "urgency"

let measure (task: Task.T) =
    match task.urgency with
    | Urgency.Urgency n -> Measure.Measurement (n)


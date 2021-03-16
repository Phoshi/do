module Tasks.Measures.UrgencyMeasure

open Tasks

let measure (task: Task.T) =
    match task.urgency with
    | Urgency.Urgency n -> Measure.Measurement (n)


module Tasks.Measures.AlwaysMeasure
open Tasks

let name = "always"

let measure (task: Task.T) =
    if task.completed |> List.isEmpty then
        Measure.Force
    else
        Measure.Neutral
module Tasks.Measures.BuryAfterCompletion

open Tasks
open Tasks.CycleRange

let measure (time: CycleTime.T) (task: Task.T) =
    Measure.neutral
module Tasks.Measures.AgeMeasure

open System
open Tasks
open Tasks.CycleRange

let name = "age"

let measure (now: DateTime) (t: Task.T) =
    if List.isEmpty t.completed then
        let days = (now - t.created).TotalDays
        Measure.Measurement (Math.Pow(days, 1.5))
    else
        match t.cycleRange with
        | CycleRange.After d ->
            let availableDate = CycleTime.toDateTime d (t.completed |> List.sortDescending |> List.head) 
            if availableDate > now then
                Measure.neutral
            else
                let days = (now - availableDate).TotalDays
                Measure.Measurement (Math.Pow(days, 1.2))
        | _ -> Measure.Neutral
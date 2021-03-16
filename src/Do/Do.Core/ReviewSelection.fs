module Do.Core.ReviewSelection

open System
open Tasks
open Tasks.Measures

let measure () =
    let now = DateTime.Now
    (Weight.weight
         Weight.aggregateByMultiplication
         [
             ImportanceMeasure.measure
             UrgencyMeasure.measure
             MomentumMeasure.measure now
             RelevanceMeasure.measure now
             CycleMeasure.measure now
         ])

let review n (tasks: Task.T list) =
    let rng = Random()
    Selector.select
        (measure ())
        (rng.Next())
        n
        tasks
        
        
let selectedTasks (options: Task.T seq) =
    let rng = Random()
    Selector.select
        (measure ())
        (rng.Next())
        1 
        (options |> List.ofSeq)
    
let taskWeight task =
    measure () task

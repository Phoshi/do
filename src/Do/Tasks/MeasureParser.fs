module Tasks.MeasureParser

open Tasks.CycleRange
open Tasks.Measures

let parse (now: System.DateTime) (text: string) =
    let annotate m t =
        (Measure.create (m t) text)
    match text with
    | "importance" -> annotate ImportanceMeasure.measure
    | "urgency" -> annotate UrgencyMeasure.measure
    | "momentum" -> annotate <| MomentumMeasure.measure now
    | "relevance" -> annotate <| RelevanceMeasure.measure now
    | "cycle" -> annotate <| CycleMeasure.measure now
    | "always" -> annotate AlwaysMeasure.measure
    | "age" -> annotate <| AgeMeasure.measure now
    | "buryAfterCompletion" -> annotate <| BuryAfterCompletion.measure (CycleTime.parse "3 days")
    | _ -> failwith "Unknown measure"
    
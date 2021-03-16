module Tasks.Confidence

type T = {
    measure: string
    confidence: double
}

let create measure confidence = {
    measure = measure
    confidence = confidence
}

let confidenceLevel measure (measures: T seq) =
    measures
    |> Seq.tryFind (fun m -> m.measure = measure)
    |> Option.map (fun m -> m.confidence)
    |> Option.defaultValue 0.0

let zero measure = create measure 0.0
let full measure = create measure 1.0


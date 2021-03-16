module Tasks.Urgency

type T = Urgency of double

let value t =
    match t with
    | Urgency d -> d

let init = Urgency 1.
let create u = Urgency u
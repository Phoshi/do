module Tasks.Importance

type T = Importance of double

let value t = match t with Importance d -> d
let init = Importance 1.
let create i = Importance i

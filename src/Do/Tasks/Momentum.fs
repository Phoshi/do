module Tasks.Momentum

type T =
    | Momentum of double
    
let init = Momentum 1.
let create m = Momentum m

let modify f m =
    match m with
    | Momentum value -> Momentum (f value)
    
let scale f m = modify ((*)f) m
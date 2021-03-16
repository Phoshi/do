module Tasks.RelevanceRange

open System

type T =
    | Always
    | After of DateTime
    | Before of DateTime
    | Between of (DateTime * DateTime)
    
let always = Always
let after dt = After dt
let before dt = Before dt
let between b a = Between (b, a)


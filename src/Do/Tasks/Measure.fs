module Tasks.Measure

open NUnit.Framework

type Measurement =
    | Suppress
    | Measurement of double
    | Force
    
let tendTowardsForce (rangeSize: double) (factor: double) =
    if factor <= 0.0 then
        Force
    else
        let ratioThroughRange = factor / rangeSize
        Measurement.Measurement (1.0/ratioThroughRange)

let neutral = Measurement 1.0

type T = Task.T -> Measurement

module Tests =
    open FsUnit
    
    let shouldBeUnder limit measure =
        match measure with
        | Measurement n -> Assert.That(n, Is.LessThan(limit))
        | _ -> Assert.Fail()
        
    let shouldBeOver limit measure =
        match measure with
        | Measurement n -> Assert.That(n, Is.GreaterThan(limit))
        | _ -> Assert.Fail()
        
    [<TestFixture>]
    type ``Given a function which tends towards forcing the measurement`` () =
        [<Test>]
        member x.``When I'm far away from the limit, the measurement is nearly neutral`` () =
            tendTowardsForce 50. 50.
            |> shouldBeUnder 1.1m
        
        [<Test>]
        member x.``When I'm at the limit, the measurment is forced`` () =
            tendTowardsForce 50. 0.
            |> should equal Force
            
        [<Test>]
        member x.``When I'm beyond the limit, the measurment is forced`` () =
            tendTowardsForce 50. -5. 
            |> should equal Force
            
        [<Test>]
        member x.``When I'm near the limit, the measurment is emphasised`` () =
            tendTowardsForce 50. 5. 
            |> shouldBeOver 1.3
            
            tendTowardsForce 5. 1.
            |> shouldBeOver 1.3
            
        [<Test>]
        member x.``When I'm very near the limit, the measurment is very emphasised`` () =
            tendTowardsForce 50. 1.
            |> shouldBeOver 1.9
            
            tendTowardsForce 5. 1. 
            |> shouldBeOver 1.9

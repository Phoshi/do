module Tasks.CycleRange

open System

module CycleTime =
    type T = {
        minutes: int
        hours: int
        days: int
        weeks: int
        months: int
        years: int
    }
    
    let create y m w d h min =
        {
            years = y
            months = m
            weeks = w
            days = d
            hours = h
            minutes = min
        }
    
    let fromTimeSpan (ts: TimeSpan) =
        create 0 0 0 (ts.Days) (ts.Hours) (ts.Minutes)
        
    let add (t: T) (dt: DateTime) =
        dt
            .AddYears(t.years)
            .AddMonths(t.months)
            .AddDays(t.weeks * 7 |> float)
            .AddDays(t.days |> float)
            .AddHours(t.hours |> float)
            .AddMinutes(t.minutes |> float)
            
    let toTimeSpan baseDate (t: T) =
        let afterDate = add t baseDate
        
        afterDate - baseDate
        
    let stringify (t: T) =
        let _s num word =
            if num = 0 then
                ""
            else if num = 1 then
                sprintf "%i %s" num word
            else
                sprintf "%i %ss" num word
        let _join (strs: string list) =
            String.Join(", ", strs)
            
        [
            _s t.years "year"
            _s t.months "month"
            _s t.weeks "week"
            _s t.days "day"
            _s t.hours "hour"
            _s t.minutes "minute"
        ]
        |> List.where (fun s -> s <> "")
        |> _join
        
    let parse (str: string) =
        let _t (split: string list) (name: string) =
            split
            |> List.tryFind (fun s -> s.Contains name)
            |> Option.map (fun s -> s.Split(" ").[0])
            |> Option.map Int32.Parse
            |> Option.defaultValue 0
            
        let split = str.Split(", ") |> List.ofSeq
        let _t = _t split
        create
            (_t "year")
            (_t "month")
            (_t "week")
            (_t "day")
            (_t "hour")
            (_t "minute")
            
    type CycleTimeComparison =
        | Before
        | On
        | After
    let clampToPrecision (time: DateTime) (t: T) =
        let clampWeek (time: DateTime) =
            let dayOfWeek =
                match time.DayOfWeek with
                | DayOfWeek.Monday -> 0.0
                | DayOfWeek.Tuesday -> 1.0
                | DayOfWeek.Wednesday -> 2.0
                | DayOfWeek.Thursday -> 3.0
                | DayOfWeek.Friday -> 4.0
                | DayOfWeek.Saturday -> 5.0
                | DayOfWeek.Sunday -> 6.0
            time.AddDays(-dayOfWeek)
            
        let _c value def conditions  =
            if List.exists (fun c -> c > 0) conditions then value else def 
        let lowPrecisionTime =
            DateTime(
                time.Year,
                _c time.Month 1 [t.months; t.weeks; t.days; t.hours; t.minutes],
                _c time.Day 1 [t.weeks; t.days; t.hours; t.minutes],
                _c time.Hour 0 [t.hours; t.minutes],
                _c time.Minute 0 [t.minutes],
                0
                )
            
        if t.weeks > 0 && t.days = 0 && t.hours = 0 && t.minutes = 0 then
            clampWeek lowPrecisionTime
        else lowPrecisionTime
        
    let toDateTime (duration: T) (dt: DateTime) =
        let normalisedDateTime = clampToPrecision dt duration
        add duration normalisedDateTime
        
    let compare (comparisonBase: DateTime) (comparisonOther: DateTime) (duration: T) =
        let comparisonReference = clampToPrecision comparisonBase duration
        let endOfPeriod = add duration comparisonReference
        let comparisonSubject = clampToPrecision comparisonOther duration
        
        if comparisonReference <= comparisonSubject && comparisonSubject < endOfPeriod then
            On
        else if endOfPeriod <= comparisonSubject then
            After
        else
            Before
        

type T =
    | Never
    | After of CycleTime.T
    | Before of CycleTime.T
    | Between of (CycleTime.T * CycleTime.T)

let never = Never
let after ts = After ts
let before ts = Before ts
let between a b = Between (a, b)

module Tests =
    open FsUnit
    open NUnit.Framework
    
    [<TestFixture>]
    type ``Given a cycle time`` () =
        [<Test>]
        member x.``if it is simple, it stringifies to only what is needed`` () =
            CycleTime.create 0 0 0 5 0 0
            |> CycleTime.stringify
            |> should equal "5 days"
            
        [<Test>]
        member x.``if it is compound, it stringifies to only what is needed`` () =
            CycleTime.create 0 0 0 5 2 0
            |> CycleTime.stringify
            |> should equal "5 days, 2 hours"
            
        [<Test>]
        member x.``if it is complex, it stringifies to only what is needed`` () =
            CycleTime.create 1 2 3 4 5 6
            |> CycleTime.stringify
            |> should equal "1 year, 2 months, 3 weeks, 4 days, 5 hours, 6 minutes"
            
        [<Test>]
        member x.``if an element is one, it stringifies singularly`` () =
            CycleTime.create 1 1 1 1 1 1
            |> CycleTime.stringify
            |> should equal "1 year, 1 month, 1 week, 1 day, 1 hour, 1 minute"
            
    [<TestFixture>]
    type ``Given a stringified cycle time`` () =
        [<Test>]
        member x.``if it is simple, it parses to the correct value`` () =
            "1 year"
            |> CycleTime.parse
            |> should equal (CycleTime.create 1 0 0 0 0 0)
            
        [<Test>]
        member x.``if it has many pieces, it parses to the correct value`` () =
            "1 year, 2 months, 3 weeks, 4 days, 5 hours, 6 minutes"
            |> CycleTime.parse
            |> should equal (CycleTime.create 1 2 3 4 5 6)
            
    [<TestFixture>]
    type ``Given two times and a range`` () =
        let now = DateTime(2020, 5, 5, 12, 31, 56)
        let ct = CycleTime.parse
        
        [<Test>]
        member this.``If the supplied time is within the same precision as the reference time plus the range, the comparison is on`` () =
            CycleTime.compare now (now.AddHours(2.0)) (ct "1 day")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddHours(-2.0)) (ct "1 day")
            |> should equal CycleTime.On
            
        [<Test>]
        member this.``If the supplied time is after the reference time plus the range, the comparison is after`` () =
            CycleTime.compare now (now.AddDays 2.0) (ct "1 day")
            |> should equal CycleTime.After
            
        [<Test>]
        member this.``If the supplied time is before the reference time plus the range, the comparison is before`` () =
            CycleTime.compare now (now.AddDays -1.0) (ct "1 day")
            |> should equal CycleTime.Before
            
        [<Test>]
        member this.``If the supplied time is outside of the same precision as the reference time plus the range, the comparison is After`` () =
            CycleTime.compare now (now.AddHours(12.0)) (ct "1 day")
            |> should equal CycleTime.After
            
        [<Test>]
        member this.``If the supplied time is before the same precision as the reference time plus the range, the comparison is Before`` () =
            CycleTime.compare now (now.AddHours(-13.0)) (ct "1 day")
            |> should equal CycleTime.Before
            
        [<Test>]
        member this.``If the cycle time is large, then the precision lowers`` () =
            CycleTime.compare now (now.AddDays(10.0)) (ct "1 month")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddDays(-4.0)) (ct "1 month")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddDays(-6.0)) (ct "1 month")
            |> should equal CycleTime.Before
            
            CycleTime.compare now (now.AddDays(30.0)) (ct "1 month")
            |> should equal CycleTime.After
            
        [<Test>]
        member this.``If the cycle time is small, then the precision raises`` () =
            CycleTime.compare now (now.AddMinutes(10.0)) (ct "1 hour")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddMinutes(-4.0)) (ct "1 hour")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddMinutes(-35.0)) (ct "1 hour")
            |> should equal CycleTime.Before
            
            CycleTime.compare now (now.AddMinutes(30.0)) (ct "1 hour")
            |> should equal CycleTime.After
        [<Test>]
        member this.``If the cycle time is measured in weeks, then the precision is also measured in weeks`` () =
            //2020-05-05 was a tuesday
            CycleTime.compare now (now.AddDays(3.0)) (ct "1 week")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddDays(-1.0)) (ct "1 week")
            |> should equal CycleTime.On
            
            CycleTime.compare now (now.AddDays(-3.0)) (ct "1 week")
            |> should equal CycleTime.Before
            
            CycleTime.compare now (now.AddDays(6.0)) (ct "1 week")
            |> should equal CycleTime.After

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

module Tasks.Selector

open System
open NUnit.Framework



let select (weight: Task.T -> Weight.T) seed (n: int) (options: Task.T list): Task.T list =
    let rec _pickTaskAt (w: double) (tasks: (Task.T * Weight.T) list) =
        let ((headTask, headWeight), tail) = (List.head tasks, List.tail tasks)
        
        let newWeight = w - (match headWeight |> Weight.result with
                             | Weight.Weight n -> n
                             | Weight.Minimum -> 0.
                             | Weight.Maximum -> w)
        
        if newWeight < 0. then
            headTask
        else
            _pickTaskAt newWeight tail
        
        
        
    let _select (rng: Random) (tasks: (Task.T * Weight.T) list) =
        match tasks |> List.tryFind (fun (_, w) -> (Weight.result w) = Weight.max) with
        | Some (essentialTask, _) -> essentialTask
        | None ->
            let weights = tasks |> List.map snd
           
            let sumOfWeights =
                Weight.sum weights 
                
            let selectedWeight = (rng.NextDouble() * sumOfWeights)
            
            _pickTaskAt selectedWeight tasks
            
    let forcedTasksPresent (tasks: (Task.T * Weight.T) list) =
        tasks
        |> List.exists (fun (_,w) -> (Weight.result w) = Weight.max)
        
    let availableTasksPresent (tasks: (Task.T * Weight.T) list) =
        tasks
        |> List.exists (fun (_,w) -> not ((Weight.result w) = Weight.min))
            
        
    let mutable weightedTasks =
        options
        |> List.map (fun t -> (t, weight t))
        
    let mutable selections = []
    let rng = Random(seed)
    while (List.length selections < n || forcedTasksPresent weightedTasks) && availableTasksPresent weightedTasks do
        let selection = _select rng weightedTasks
        
        selections <- selection :: selections
        weightedTasks <- weightedTasks |> List.filter (fun (t, _) -> not (t = selection))
        
    selections
    
module Tests =
    open FsUnit
    let ts = Task.Tests.newTask |> Task.setTitle "suppress"
    let tf = Task.Tests.newTask |> Task.setTitle "force"
    let tc weight = Task.Tests.newTask |> Task.setTitle (weight.ToString())
    let rng = Random()
    let t weight = tc weight |> Task.setDescription (rng.Next().ToString())
    let genericTaskForced () = tf |> Task.setDescription (rng.Next().ToString())
    let w (task: Task.T) =
        if task.title = "force" then (Weight.create Weight.Maximum [])
        else if task.title = "suppress" then (Weight.create Weight.Minimum [])
        else Weight.create (Weight.Weight (Double.Parse(task.title))) []
    
    [<TestFixture>]
    type ``Given a series of weighted tasks`` () =
        [<Test>]
        member x.``heavier tasks are more likely to show up`` () =
            select w 17 3 [t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; tc 25]
            |> List.contains (tc 25)
            |> should be True
            
        [<Test>]
        member x.``lighter tasks can still show up`` () =
            select w 173 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; tc 25]
            |> List.contains (tc 0.5)
            |> should be True
            
        [<Test>]
        member x.``heavier tasks don't _have_ to show up`` () =
            let r = Random(456430866)
            let seeds = List.init 1000 (fun _ -> r.Next())
            
            let mutable matches = 0
            for seed in seeds do
                let bigFound =
                    select w seed 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; tc 25]
                    |> List.contains (tc 25)
                    
                if bigFound then
                    matches <- matches + 1
                    
            matches
            |> should lessThan 1000
            
        [<Test>]
        member x.``forced tasks show up even if it means they exceed the limiters`` () =
            let r = Random(456430866)
            let seeds = List.init 1000 (fun _ -> r.Next())
            
            let mutable matches = 0
            for seed in seeds do
                let len =
                    select w seed 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; genericTaskForced (); genericTaskForced (); genericTaskForced (); genericTaskForced ()]
                    |> List.length
                    
                if len = 4 then
                    matches <- matches + 1
                    
            matches
            |> should equal 1000
            
        [<Test>]
        member x.``forced tasks do _have_ to show up`` () =
            let r = Random(456430866)
            let seeds = List.init 1000 (fun _ -> r.Next())
            
            let mutable matches = 0
            for seed in seeds do
                let bigFound =
                    select w seed 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; tf]
                    |> List.contains (tf)
                    
                if bigFound then
                    matches <- matches + 1
                    
            matches
            |> should equal 1000
            
        [<Test>]
        member x.``suppressed tasks never show up`` () =
            let r = Random(456430866)
            let seeds = List.init 1000 (fun _ -> r.Next())
            
            let mutable matches = 0
            for seed in seeds do
                let bigFound =
                    select w seed 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; ts]
                    |> List.contains (ts)
                    
                if bigFound then
                    matches <- matches + 1
                    
            matches
            |> should equal 0
            
        [<Test>]
        member x.``but heavier tasks usually show up`` () =
            let r = Random(456430866)
            let seeds = List.init 1000 (fun _ -> r.Next())
            
            let mutable matches = 0
            for seed in seeds do
                let bigFound =
                    select w seed 3 [t 1; t 1; t 1; t 1; tc 0.5; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; t 1; tc 25]
                    |> List.contains (tc 25)
                    
                if bigFound then
                    matches <- matches + 1
                    
            matches
            |> should greaterThanOrEqualTo 900
            
        
        
            

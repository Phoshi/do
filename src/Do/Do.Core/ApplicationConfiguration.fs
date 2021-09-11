module Do.Core.ApplicationConfiguration

open System.IO
open MarkdownSource

let settingsPath () =
    let appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
    Path.Combine(appData, "do", "settings.yaml")

let load () =
    File.ReadAllText(settingsPath ())
    |> fun p -> MetadataSource().Metadata<ApplicationConfiguration>(p)
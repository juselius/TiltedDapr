open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext()

let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let testPath   = Path.getFullName "test"
let libPath    = None

let distPath = Path.getFullName "dist"
let packPath = Path.getFullName "packages"
let versionFile = Path.getFullName ".version"

let vite = "node ../../node_modules/vite/bin/vite.js -c ../../vite.config.js"
Target.create "Clean" (fun _ -> Shell.cleanDir distPath)

Target.create "InstallClient" (fun _ ->
    run npm "install" "."
    run dotnet "tool restore" "."
)

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o {distPath}" serverPath
      "client", dotnet $"fable -o build/client --run {vite} build --outDir {distPath}/public" clientPath ]
    |> runParallel
)

Target.create "BundleDebug" (fun _ ->
    [ "server", dotnet $"publish -c Debug -o {distPath}" serverPath
      "client", dotnet $"fable -o build/client --run {vite} build --outDir {distPath}/public" clientPath ]
    |> runParallel
)

Target.create "Pack" (fun _ ->
    match libPath with
    | Some p -> run dotnet $"pack -c Release -o \"{packPath}\"" p
    | None -> ()
)

Target.create "Run" (fun _ ->
    [ "server", dotnet "watch run" serverPath
      "client", dotnet $"fable watch -o build/client --run {vite}" clientPath ]
    |> runParallel
)

Target.create "Server" (fun _ -> run dotnet "watch run" serverPath)

Target.create "ServerWatch" (fun _ -> run dotnet "watch build" serverPath)

Target.create "Client" (fun _ ->
    run dotnet $"fable watch -o build/client --run {vite}" clientPath
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

Target.create "Test" (fun _ ->
    if System.IO.Directory.Exists testPath then
        [ "server", dotnet "run" (testPath + "/Server")
          "client", dotnet $"fable -o build/client --run {vite} build" (testPath + "/Client") ]
        |> runParallel
    else ()
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"

    "Clean"
        ==> "BundleDebug"

    "Clean"
        ==> "Test"

    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "Clean"
        ==> "Pack"

    "Server"

    "ServerWatch"

    "Client"

]

[<EntryPoint>]
let main args = runOrDefault args
module Settings

open System.IO
open Thoth.Json.Net
open Serilog

type Settings =
    {
        setting: string
    }

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
        | null
        | "" -> None
        | x -> Some x

let appsettings =
    let settings = File.ReadAllText "appsettings.json"
    match Decode.Auto.fromString<Settings> settings with
    | Ok s -> s
    | Error e -> failwith e

// server home
let contentRoot =
    tryGetEnv "SERVER_CONTENT_ROOT"
    |> function
        | Some root -> Path.GetFullPath root
        | None -> Path.GetFullPath "../Client"

// webfiles and servable assets
let webRoot =
    tryGetEnv "SERVER_WEB_ROOT"
    |> function
        | Some root -> Path.GetFullPath root
        | None -> Path.Join [| contentRoot; "public" |]

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map int
    |> Option.defaultValue 8085

let listenAddress = "http://0.0.0.0:" + port.ToString()

Log.Information $"Listen: {listenAddress}"
Log.Debug $"Public webroot: {webRoot}"
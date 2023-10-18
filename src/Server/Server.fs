module Server

open System
open Microsoft.AspNetCore.Http
open FSharp.Control
open Saturn
open Giraffe
open Serilog
open Serilog.Events
open Argu

open Shared

type Arguments =
    | Log_Level of level: int
    | Port of port: int
    | [<MainCommand; Last>] Dir of path: string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Log_Level _ -> "0=Error, 1=Warning, 2=Info, 3=Debug, 4=Verbose"
            | Port _ -> "listen port (default 8085)"
            | Dir _ -> "serve from dir"

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new project")
|> ignore

storage.AddTodo(Todo.create "Write your app")
|> ignore

storage.AddTodo(Todo.create "Ship it !!!")
|> ignore

let private getTodos next ctx =
    task {
        let todos = storage.GetTodos()
        return! json todos next ctx
    }

let private addTodo (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! todo = ctx.BindJsonAsync<Todo>()

        match storage.AddTodo todo with
        | Ok () -> return! json todo next ctx
        | Error e -> return! RequestErrors.BAD_REQUEST "fail" next ctx
    }

let webApp =
    choose [
        GET >=> route "/api/getTodos" >=> getTodos
        POST >=> route "/api/addTodo" >=> addTodo
    ]

let configureSerilog level =
    let n =
        match level with
        | 0 -> LogEventLevel.Error
        | 1 -> LogEventLevel.Warning
        | 2 -> LogEventLevel.Information
        | 3 -> LogEventLevel.Debug
        | _ -> LogEventLevel.Verbose
    LoggerConfiguration()
        .MinimumLevel.Is(n)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Filter.ByExcluding("RequestPath like '/health%'")
        .WriteTo.Console()
        .CreateLogger()

let app port dir =
    application {
        url $"http://0.0.0.0:{port}"
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
        logging (fun logger -> logger.AddSerilog() |> ignore)
    }

let colorizer =
    function
    | ErrorCode.HelpText -> None
    | _ -> Some ConsoleColor.Red

let errorHandler = ProcessExiter(colorizer = colorizer)

[<EntryPoint>]
let main argv =
    let parser =
        ArgumentParser.Create<Arguments>(programName = "TiltedDapr", errorHandler = errorHandler)
    let args = parser.Parse argv

    Log.Logger <- configureSerilog (args.GetResult(Log_Level, defaultValue = 2))

    let port = args.GetResult (Port, defaultValue = Settings.port)
    let dir = args.GetResult (Dir, defaultValue = ".")

    run (app port dir)
    0
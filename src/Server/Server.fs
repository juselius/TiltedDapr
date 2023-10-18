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
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Log_Level _ -> "0=Error, 1=Warning, 2=Info, 3=Debug, 4=Verbose"

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

let testEventHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! msg = ctx.BindJsonAsync<string>()
        Log.Information $"test event received: {msg}"
        return! text msg next ctx
    }

let intraApp =
    choose [
        route "/test-events" >=> choose [
                OPTIONS >=> Successful.NO_CONTENT // subscribe to dapr events on route
                POST >=> testEventHandler
            ]
        RequestErrors.NOT_FOUND "Not Found"
    ]

let webApp =
    choose [
        routePorts [ Settings.intraPort, intraApp ]
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

let app =
    application {
        url $"http://{Settings.appHost}"
        url $"http://{Settings.intraHost}"
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
    run app
    0
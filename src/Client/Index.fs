module Index

open Thoth.Json
open Thoth.Fetch
open Feliz
open Feliz.Bulma
open Feliz.Router

open Shared

type Model = { CurrentUrl: string list; Todos: Todo list; Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo of Todo

let getTodos dispatch =
    let decoder: Decoder<Todo list> =
        Thoth.Json.Decode.Auto.generateDecoder ()

    promise {
        let! res = Fetch.fetchAs (url = "/api/getTodos", decoder = decoder)
        dispatch (GotTodos res)
    }
    |> Promise.start

let addTodo (input: string) dispatch =
    let decoder: Decoder<Todo> = Thoth.Json.Decode.Auto.generateDecoder ()
    let todo = Todo.create input

    promise {
        let! res = Fetch.post (url = "/api/addTodo", data = todo, decoder = decoder)
        dispatch (AddTodo res)
    }
    |> Promise.start

let init () : Model =
    { CurrentUrl = []; Todos = []; Input = "" }

let update (model: Model) (msg: Msg) : Model =
    match msg with
    | GotTodos todos -> { model with Todos = todos }
    | SetInput value -> { model with Input = value }
    | AddTodo todo -> { model with Todos = model.Todos @ [ todo ] }

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ol [
                for todo in model.Todos do
                    Html.li [ prop.text todo.Description ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun x -> SetInput x |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Todo.isValid model.Input |> not)
                        prop.onClick (fun _ -> addTodo model.Input dispatch)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "foo"
                            ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]

let app =
    Fable.React.FunctionComponent.Of (fun () ->
        let currentUrl, setUrl = React.useState (Router.currentUrl ())

        let initialModel = init ()
        let model, dispatch = React.useReducer (update, initialModel)

        React.useEffect ((fun _ -> getTodos dispatch), [||])

        Html.div [
            React.router [
                router.onUrlChanged (setUrl)
            ]
            match currentUrl with
            | _ -> view model dispatch
        ])

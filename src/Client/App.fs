module App

open Fable.Core.JsInterop
open Browser.Dom
open Feliz

importSideEffects "./public/style.scss"

let elem = document.getElementById "feliz-app"
let root = ReactDOM.createRoot elem
root.render (Index.app ())
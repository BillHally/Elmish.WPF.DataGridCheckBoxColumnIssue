#r "WindowsBase"
#r "PresentationCore"
#r "PresentationFramework"
#r "System.Xaml"

#if USE_FIX

printfn "Using fix..."

#r @"packages\Latest\Elmish\lib\netstandard2.0\elmish.dll"
#I @"paket-files\fix\BillHally\Elmish.WPF\src\Elmish.WPF"

#load @"Utils.fs"
#load @"Types.fs"
#load @"Config.fs"
#load @"Binding.fs"
#load @"ViewModel.fs"
#load @"ViewModelUtilities.fs"
#load @"Program.fs"

#else

printfn "Using latest release..."

#r @"packages\Latest\Elmish\lib\netstandard2.0\elmish.dll"
#r @"packages\Latest\Elmish.WPF\lib\net461\elmish.wpf.dll"

#endif

open System
open System.Diagnostics

open Elmish
open Elmish.WPF

module Wpf =

    open System.Windows
    open System.Windows.Controls
    open System.Windows.Data

    let bind1 property path (x : FrameworkElement) =
        let binding = Binding(path, Mode=BindingMode.OneWay)
        x.SetBinding(property, binding) |> ignore

    let bind2 property path (x : FrameworkElement) =
        let binding = Binding(path, Mode=BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged)
        x.SetBinding(property, binding) |> ignore

    let textBlock (path : string) =
        let tb = TextBlock()
        tb |> bind1 TextBlock.TextProperty path
        tb :> FrameworkElement

    let textBox (path : string) =
        let tb = TextBox(Margin=Thickness(5.0))
        tb |> bind2 TextBox.TextProperty path
        tb :> FrameworkElement

    let stackPanel xs =
        let sp = StackPanel()
        xs |> Seq.iter (sp.Children.Add >> ignore)
        sp :> FrameworkElement

    let dataGrid itemsPath columns =
        let dg = DataGrid(AutoGenerateColumns=false, Margin=Thickness(5.0), MinHeight=100.0)
        dg |> bind1 DataGrid.ItemsSourceProperty itemsPath
        columns |> Seq.iter dg.Columns.Add
        dg :> FrameworkElement

    let dgText header path =
        DataGridTextColumn(Header=header, Binding=Binding(path))
        :> DataGridColumn

    let dgCheckBox header path =
        DataGridCheckBoxColumn(Header=header, Binding=Binding(path, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged))
        :> DataGridColumn

    let window w h x =
        Window(Content = x, Width=w, Height=h)

module App =
    type SubModel =
        {
            Name : string
            B    : bool
        }

    type Model =
        {
            A : string
            SubModels : SubModel[]
        }

    let init () =
        {
            A = "Initial value"
            SubModels =
                [|
                    { Name = "Initially true" ; B = true  }
                    { Name = "Initially false"; B = false }
                |]
        }

    type Msg =
        | SetA of string
        | SetSub of string * bool

    let update msg m =
        match msg with
        | SetA x ->
            {
                m with
                    A = x
            }
        | SetSub (name, value) ->
            {
                m with
                    SubModels =
                        m.SubModels
                        |> Array.map
                            (
                                function
                                | x when x.Name = name -> { x with B = value }
                                | x -> x
                            )
            }

module Bindings =
    open App

    let subBindings () =
        [
            "Name" |> Binding.oneWay (fun (m, s) -> s.Name)
            "B"    |> Binding.twoWay (fun (m, s) -> s.B   ) (fun v (m, s) -> SetSub (s.Name, v))
        ]

    let rootBindings model dispatch =
        [
            "A"         |> Binding.twoWay (fun m -> m.A) (fun v m -> SetA v)
            "SubModels" |> Binding.subBindingSeq id (fun m -> m.SubModels) (fun s -> s.Name) subBindings
        ]

open Wpf

let mainWindow =
    window 300.0 250.0
        (
        stackPanel
            [|
                textBox "A"
                (
                    dataGrid
                        "SubModels"

                        [|
                            dgText "Name" "Name"
                            dgCheckBox "B" "B"
                        |]
                )
            |]
        )

let traceListener = new TextWriterTraceListener(Console.Out)    
PresentationTraceSources.DataBindingSource.Listeners.Add(traceListener)
PresentationTraceSources.SetTraceLevel(mainWindow, PresentationTraceLevel.High)

if isNull System.Windows.Application.Current then
    System.Windows.Application() |> ignore

Program.mkSimple App.init App.update Bindings.rootBindings
|> Program.withConsoleTrace
|> Program.runWindowWithConfig
  { ElmConfig.Default with LogConsole = true }
  mainWindow

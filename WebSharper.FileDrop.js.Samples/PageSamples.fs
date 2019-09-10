namespace WebSharper.FileDropJsSamples.Pages

open WebSharper
//open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

open WebSharper.FileDropJsSamples.Components
open WebSharper.FileDropJs
open WebSharper.FileDropJsSamples.Config.Route
open WebSharper.FileDropJsSamples

[<JavaScript>]
module PageSamples =

    type private template = Templating.Template<"PageSamples.html">

    let private refreshData() =
        async {
            //Console.Log "complete"
            ()
        } |> Async.Start

    let private uploadPath () =
        Helpers.Link UploadAttachment


    let Main router =
        let progressBar = ProgressBar.ProgressBarT()
        let progressBarComp = progressBar.ProgressBar()
        let rvAlert = Var.Create (Alert.Success "")
        let alertBox = Alert.Alert rvAlert

        let registerOnReadyFn (elem:WebSharper.JavaScript.Dom.Element) = 
            let dd = new FileDrop("uploadWidget")
            dd.Event("send", fun (files:FileList) ->
                files.Each(fun (file:File) ->
                    //resets the progress bar for each file
                    file.Event("xhrSend", fun xhr data opt ->
                        progressBar.Restart()
                    )
                    file.Event("progress", fun current total xhr ev ->
                        let width = current / total * 100.
                        progressBar.Step width
                    )

                    file.Event("done", fun (xhr:WebSharper.JavaScript.XMLHttpRequest) ev ->
                        refreshData()
                        progressBar.Hide()
                    )
                    file.Event("error", fun evt (xhr:WebSharper.JavaScript.XMLHttpRequest) ->
                        Var.Set rvAlert (Alert.Danger [ xhr.StatusText ])
                        progressBar.Hide()
                    )
                    file.SendTo(uploadPath()) |> ignore
                    ()
                )
                |> ignore
            )
            |> ignore

        let cnt = 
            template()
                .ProgressBarComponent(progressBarComp)
                .AlertBoxComponent(alertBox)
                .Doc()
            //:?> Elt
        
        // Not working! It throws TypeError: cnt.OnAfterRender is not a function
        //cnt.OnAfterRender registerOnReadyFn |> ignore
        //cnt :> Doc

        div [ on.afterRender registerOnReadyFn ] [ cnt ]

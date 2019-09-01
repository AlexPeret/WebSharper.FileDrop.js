namespace WebSharper.FileDropJsSamples.Pages

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.FileDropJs

[<JavaScript>]
module PageSamples =

    let Main () =
        let registerOnReadyFn (elem:Dom.Element) = 
            let dd = new FileDrop("uploadWidget")
            dd.Event("send", fun files ->
                files.Each(fun file ->
                    // reseta a barra de progresso para cada arquivo
                    file.Event("xhrSend", fun xhr data opt ->
                        progressBar.Restart ()
                    )
                    file.Event("progress", fun current total xhr ev ->
                        let width = current / total * 100.
                        progressBar.Step width
                    )

                    file.Event("done", fun (xhr:XMLHttpRequest) ev ->
                        UploadCallback loadAttachmentsCallback
                        progressBar.Hide()
                    )
                    file.Event("error", fun evt (xhr:XMLHttpRequest) ->
                        Var.Set rvStatusMsg (Some <| Result.Error [ xhr.StatusText ])
                        progressBar.Hide()
                    )
                    file.SendTo(uploadPathCallback():string) |> ignore
                )
                |> ignore
            )
            |> ignore


        Doc.Empty

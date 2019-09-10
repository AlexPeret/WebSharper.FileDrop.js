namespace WebSharper.FileDropJsSamples

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

open WebSharper.FileDropJsSamples.Config.Route

//type EndPoint =
//    | [<EndPoint "/">] Samples

module Templating =
    open WebSharper.UI.Html

    type MainTemplate = Templating.Template<"Main.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Samples" => EndPoint.Samples
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )

module Site =
    open WebSharper.UI.Html
    open WebSharper.UI.Client
    open WebSharper.FileDropJsSamples
     
    let private UploadHandler ctx =
        let salvarCallback n m fs = 
            System.Threading.Thread.Sleep(2000)
            //Error [ "not implemented" ]
            Ok "file saved"

        Server.UploadContent ctx salvarCallback
    
    [<JavaScript>]
    let RouteClientPage () =
        let router = InstallRouter()
        let routeTo = Helpers.RouteTo router

        let doc =
            router.View
            |> View.Map (fun endpoint -> 
                match endpoint with
                | EndPoint.Samples -> Pages.PageSamples.Main routeTo
                | _ -> div [ ] [ text "not found" ]
            )
            |> Doc.EmbedView
        doc


    let LoadClientPage ctx endpoint title =
        let body = client <@ RouteClientPage () @>
        Templating.Main ctx endpoint title [ body ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->

            match endpoint with
            | EndPoint.Samples -> 
                LoadClientPage ctx endpoint "Samples"

            | EndPoint.PageError -> 
                Templating.Main ctx EndPoint.PageError "Error Page" [
                    div [] [ text "replace by error page" ]
                ]

            | UploadAttachment -> UploadHandler ctx
        )

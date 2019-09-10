namespace WebSharper.FileDropJsSamples.Config

//open System
open WebSharper
//open WebSharper.Resources
open WebSharper.Sitelets
open WebSharper.UI

module Route =

    [<JavaScript>]
    type EndPoint =
        | [<EndPoint "/">] Samples
        | [<EndPoint "/error">] PageError
        | [<EndPoint "POST /upload-attachment">] UploadAttachment

    [<JavaScript>]
    let SiteRouter : Router<EndPoint> =
        Router.Infer()

    [<JavaScript>]
    let InstallRouter () =
        Router.Install PageError SiteRouter

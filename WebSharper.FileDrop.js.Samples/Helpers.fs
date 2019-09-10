namespace WebSharper.FileDropJsSamples

open System
open System.IO
open WebSharper
open WebSharper.Sitelets
open WebSharper.Sitelets.Http
open WebSharper.UI
open WebSharper.JavaScript

module Helpers =
    open WebSharper.FileDropJsSamples.Config.Route

    let ReadLines (filePath:string) =
        let reader = 
            new StreamReader(filePath) 
            |> Seq.unfold (fun sr -> 
                match sr.ReadLine() with
                | null -> sr.Dispose(); None 
                | str -> Some(str, sr))
        reader |> Seq.toList

    let readBuffer (data:Stream) =
        fun (out:Stream) ->
            let buffer = Array.zeroCreate (16 * 1024)
            let rec loop () =
                let read = data.Read(buffer, 0, buffer.Length)
                if read > 0 then out.Write(buffer, 0, read); loop ()
            loop ()

    let GetHeaderValue (ctx:Context<EndPoint>) key =
        ctx.Request.Headers 
        |> Seq.filter (fun h -> h.Name = key)
        |> Seq.map (fun h -> System.Web.HttpUtility.UrlDecode(h.Value))
        |> Seq.exactlyOne

    let ResultToContent res = 
        match res with
        | Error (erro:string list) -> 
            let errorStream = new MemoryStream()
            let sw = new StreamWriter(errorStream)

            erro 
            |> List.iter(fun e -> sw.Write(e))

            sw.Flush()
            errorStream.Position <- 0L

            Content.Custom(
                Status = Status.InternalServerError,
                Headers = [],
                WriteBody = (readBuffer errorStream)) 
        | Ok status ->
            Content.Text status

    [<JavaScript>]
    let Link act =
        Router.Link SiteRouter act

    [<JavaScript>]
    let RouteTo router = 
        Var.Set router

    [<JavaScript>]
    module JS =
    
        [<Inline "document.location = $location">]
        let Redirect (location: string) = X<unit>

        let Sleep miliseconds =
            Promise(fun (resolve, _) ->
                JS.SetTimeout (fun () -> resolve 42) miliseconds |> ignore)
            |> Promise.AsTask
            |> Async.AwaitTask


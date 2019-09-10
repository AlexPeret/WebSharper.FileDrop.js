namespace WebSharper.FileDropJsSamples

open WebSharper
open WebSharper.Sitelets

module Server =
    open WebSharper.FileDropJsSamples.Config.Route
    open WebSharper.FileDropJsSamples.Helpers

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }

    let UploadContent (ctx:Context<EndPoint>) saveCallback =
        let mime = GetHeaderValue ctx "X-File-Type"

        match Array.ofSeq ctx.Request.Files with
        | [| f |] ->
            use fileStream = new System.IO.MemoryStream()
            f.InputStream.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
            f.InputStream.CopyTo(fileStream)
            let res = saveCallback f.FileName mime fileStream

            fileStream.Close()

            ResultToContent res
        | _ ->
            match ctx.Request.Body.Length with
            | 0L ->
                Content.Text "The file is required"
                |> Content.SetStatus (Http.Status.Custom 400 (Some "Bad Request"))
            | _ ->
                let fileName = 
                    ctx.Request.Headers 
                    |> Seq.filter (fun h -> h.Name = "X-File-Name")
                    |> Seq.map (fun h -> System.Web.HttpUtility.UrlDecode(h.Value))
                    |> Seq.exactlyOne

                use fileStream = new System.IO.MemoryStream()
                ctx.Request.Body.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
                ctx.Request.Body.CopyTo(fileStream)

                let res = saveCallback fileName mime fileStream

                fileStream.Close()

                ResultToContent res



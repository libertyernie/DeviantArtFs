﻿namespace DeviantArtFs.Requests.Collections

open System
open System.IO
open DeviantArtFs
open DeviantArtFs.Interop

module CreateCollectionFolder =
    let AsyncExecute token (folder: string) = async {
        let query = seq {
            yield sprintf "folder=%s" (dafs.urlEncode folder)
        }

        let req = dafs.createRequest token "https://www.deviantart.com/api/v1/oauth2/collections/folders/create"
        req.Method <- "POST"
        req.ContentType <- "application/x-www-form-urlencoded"

        do! async {
            use! stream = req.GetRequestStreamAsync() |> Async.AwaitTask
            use sw = new StreamWriter(stream)
            do! String.concat "&" query |> sw.WriteAsync |> Async.AwaitTask
        }

        let! json = dafs.asyncRead req
        return FoldersCreateResponse.Parse json
    }

    let ExecuteAsync token folder = Async.StartAsTask (async {
        let! f = AsyncExecute token folder
        return {
            new IDeviantArtFolder with
                member __.Folderid = f.Folderid
                member __.Parent = Nullable()
                member __.Name = f.Name
                member __.Size = Nullable()
        }
    })
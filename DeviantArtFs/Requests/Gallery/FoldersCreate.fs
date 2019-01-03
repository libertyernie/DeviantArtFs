﻿namespace DeviantArtFs.Requests.Gallery

open System
open System.IO
open DeviantArtFs
open FSharp.Data

type internal FoldersCreateResponse = JsonProvider<"""{
    "folderid": "E431BAFB-7A00-7EA1-EED7-2EF9FA0F04CE",
    "name": "My New Gallery"
}""">

type FoldersCreateResult = {
    Folderid: Guid
    Name: string
}

module FoldersCreate =
    let AsyncExecute token (folder: string) = async {
        let query = seq {
            yield sprintf "folder=%s" (dafs.urlEncode folder)
        }

        let req = dafs.createRequest token "https://www.deviantart.com/api/v1/oauth2/gallery/folders/create"
        req.Method <- "POST"
        req.ContentType <- "application/x-www-form-urlencoded"

        do! async {
            use! stream = req.GetRequestStreamAsync() |> Async.AwaitTask
            use sw = new StreamWriter(stream)
            do! String.concat "&" query |> sw.WriteAsync |> Async.AwaitTask
        }

        let! json = dafs.asyncRead req
        let resp = FoldersCreateResponse.Parse json

        return {
            Folderid = resp.Folderid
            Name = resp.Name
        }
    }

    let ExecuteAsync token folder = AsyncExecute token folder |> Async.StartAsTask
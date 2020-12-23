﻿namespace DeviantArtFs.Api.Collections

open System.IO
open DeviantArtFs

module CreateCollectionFolder =
    let AsyncExecute token (folder: string) = async {
        let query = seq {
            yield sprintf "folder=%s" (Dafs.urlEncode folder)
        }

        let req = Dafs.createRequest token DeviantArtCommonParams.Default "https://www.deviantart.com/api/v1/oauth2/collections/folders/create"
        req.Method <- "POST"
        req.ContentType <- "application/x-www-form-urlencoded"

        req.RequestBodyText <- String.concat "&" query

        let! json = Dafs.asyncRead req
        return DeviantArtCollectionFolder.Parse json
    }

    let ExecuteAsync token folder =
        AsyncExecute token folder
        |> Async.StartAsTask
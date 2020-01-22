﻿namespace DeviantArtFs.Requests.Browse

open DeviantArtFs
open FSharp.Control

type NewestRequest() =
    member val CategoryPath = null with get, set
    member val Q = null with get, set

module Newest =
    let AsyncExecute token (paging: IDeviantArtPagingParams) (req: NewestRequest) = async {
        let query = seq {
            match Option.ofObj req.CategoryPath with
            | Some s -> yield sprintf "category_path=%s" (Dafs.urlEncode s)
            | None -> ()
            match Option.ofObj req.Q with
            | Some s -> yield sprintf "q=%s" (Dafs.urlEncode s)
            | None -> ()
            yield! QueryFor.paging paging 120
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/browse/newest?%s"
            |> Dafs.createRequest token
        let! json = Dafs.asyncRead req
        return DeviantArtBrowsePagedResult.Parse json
    }

    let ToAsyncSeq token offset req =
        Dafs.getMax AsyncExecute token
        |> Dafs.toAsyncSeq offset req

    let ToArrayAsync token offset limit req =
        ToAsyncSeq token offset req
        |> AsyncSeq.take limit
        |> AsyncSeq.map (fun o -> o :> IBclDeviation)
        |> AsyncSeq.toArrayAsync
        |> Async.StartAsTask

    let ExecuteAsync token paging req =
        AsyncExecute token paging req
        |> AsyncThen.map (fun p -> p :> IBclDeviantArtBrowsePagedResult)
        |> Async.StartAsTask
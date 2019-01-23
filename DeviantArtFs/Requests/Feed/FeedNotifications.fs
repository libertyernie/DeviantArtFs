﻿namespace DeviantArtFs.Requests.Feed

open DeviantArtFs

module FeedNotifications =
    open FSharp.Control
    open System

    let AsyncExecute token cursor = async {
        let query = seq {
            match cursor with
            | Some s -> yield sprintf "cursor=%s" (dafs.urlEncode s)
            | None -> ()
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/feed/notifications?%s"
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        return DeviantArtCursorResult<DeviantArtFeedNotification>.Parse json
    }

    let ToAsyncSeq token cursor = AsyncExecute token |> dafs.cursorToAsyncSeq cursor

    let ToArrayAsync token cursor limit =
        cursor
        |> Option.ofObj
        |> ToAsyncSeq token
        |> AsyncSeq.take limit
        |> AsyncSeq.map (fun o -> o :> IBclDeviantArtFeedNotification)
        |> AsyncSeq.toArrayAsync
        |> Async.StartAsTask

    let ExecuteAsync token cursor =
        cursor
        |> Option.ofObj
        |> AsyncExecute token
        |> iop.thenMapCursorResult (fun o -> o :> IBclDeviantArtFeedNotification)
        |> Async.StartAsTask
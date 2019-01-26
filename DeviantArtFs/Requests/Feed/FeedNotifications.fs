﻿namespace DeviantArtFs.Requests.Feed

open DeviantArtFs

module FeedNotifications =
    open FSharp.Control
    open System

    let AsyncExecute token cursor = async {
        let query = seq {
            match cursor with
            | Some s -> yield sprintf "cursor=%s" (Dafs.urlEncode s)
            | None -> ()
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/feed/notifications?%s"
            |> Dafs.createRequest token
        let! json = Dafs.asyncRead req
        return DeviantArtFeedCursorResult<DeviantArtFeedItem>.Parse json
    }

    let ToAsyncSeq token cursor = AsyncExecute token |> Dafs.cursorToAsyncSeq cursor

    let ToArrayAsync token cursor limit =
        cursor
        |> Option.ofObj
        |> ToAsyncSeq token
        |> AsyncSeq.take limit
        |> AsyncSeq.map (fun o -> o :> IBclDeviantArtFeedItem)
        |> AsyncSeq.toArrayAsync
        |> Async.StartAsTask

    let ExecuteAsync token cursor =
        cursor
        |> Option.ofObj
        |> AsyncExecute token
        |> AsyncThen.mapFeedCursorResult (fun o -> o :> IBclDeviantArtFeedItem)
        |> Async.StartAsTask
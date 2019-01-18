﻿namespace DeviantArtFs.Requests.Comments

open DeviantArtFs
open System
open FSharp.Control

type StatusCommentsRequest(statusid: Guid) =
    member __.Statusid = statusid
    member val Commentid = Nullable<Guid>() with get, set
    member val Maxdepth = 0 with get, set

module StatusComments =
    let AsyncExecute token (paging: PagingParams) (req: StatusCommentsRequest) = async {
        let query = seq {
            match Option.ofNullable req.Commentid with
            | Some s -> yield sprintf "commentid=%O" s
            | None -> ()
            yield sprintf "maxdepth=%d" req.Maxdepth
            yield! paging.GetQuery()
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/comments/status/%O?%s" req.Statusid
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        return json |> DeviantArtCommentPagedResult.Parse
    }

    let ToAsyncSeq token req offset = AsyncExecute token |> dafs.toAsyncSeq offset 50 req

    let ToArrayAsync token req offset limit =
        ToAsyncSeq token req offset
        |> AsyncSeq.take limit
        |> AsyncSeq.map (fun c -> c :> IBclDeviantArtComment)
        |> AsyncSeq.toArrayAsync
        |> Async.StartAsTask

    let ExecuteAsync token paging req = AsyncExecute token paging req |> iop.thenTo (fun r -> r :> IBclDeviantArtCommentPagedResult) |> Async.StartAsTask
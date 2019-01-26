﻿namespace DeviantArtFs.Requests.Gallery

open System
open DeviantArtFs

type GalleryRequestMode = Popular=1 | Newest=2

type GalleryByIdRequest(folderid: Guid) =
    member __.Folderid = folderid
    member val Username = null with get, set
    member val Mode = GalleryRequestMode.Popular with get, set

module GalleryById =
    open FSharp.Control

    let AsyncExecute token (paging: IDeviantArtPagingParams) (req: GalleryByIdRequest) = async {
        let query = seq {
            match Option.ofObj req.Username with
            | Some s -> yield sprintf "username=%s" (dafs.urlEncode s)
            | None -> ()
            yield sprintf "mode=%s" (if req.Mode = GalleryRequestMode.Newest then "newest" else "popular")
            yield! queryFor.paging paging
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/gallery/%A?%s" req.Folderid
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        return DeviantArtPagedResult<Deviation>.Parse json
    }

    let ToAsyncSeq token req offset = AsyncExecute token |> dafs.toAsyncSeq offset 24 req

    let ToArrayAsync token req offset limit =
        ToAsyncSeq token req offset
        |> AsyncSeq.take limit
        |> AsyncSeq.map (fun o -> o :> IBclDeviation)
        |> AsyncSeq.toArrayAsync
        |> Async.StartAsTask

    let ExecuteAsync token paging req = AsyncExecute token paging req |> AsyncThen.mapPagedResult (fun o -> o :> IBclDeviation) |> Async.StartAsTask

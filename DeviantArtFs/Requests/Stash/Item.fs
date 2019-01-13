﻿namespace DeviantArtFs.Requests.Stash

open DeviantArtFs
open DeviantArtFs.Interop
open FSharp.Data

type ItemRequest(itemid: int64) = 
    member __.Itemid = itemid
    member val ExtParams = new ExtParams() with get, set

module Item =
    let AsyncExecute token (req: ItemRequest) = async {
        let query = seq {
            yield sprintf "ext_submission=%b" req.ExtParams.ExtSubmission
            yield sprintf "ext_camera=%b" req.ExtParams.ExtCamera
            yield sprintf "ext_stats=%b" req.ExtParams.ExtStats
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/stash/item/%d?%s" req.Itemid
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        return StashMetadataResponse.Parse json |> StashMetadata
    }

    let ExecuteAsync token req = AsyncExecute token req |> iop.thenTo (fun i -> i :> IBclStashMetadata) |> Async.StartAsTask
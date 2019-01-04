﻿namespace DeviantArtFs.Requests.Deviation

open DeviantArtFs
open DeviantArtFs.Interop
open System

type MetadataRequest(deviationids: seq<Guid>) =
    member __.Deviationids = deviationids
    member val ExtParams = new ExtParams() with get, set
    member val ExtCollection = false with get, set

module MetadataById =
    let AsyncExecute token (req: MetadataRequest) = async {
        let query = seq {
            yield sprintf "ext_submission=%b" req.ExtParams.ExtSubmission
            yield sprintf "ext_camera=%b" req.ExtParams.ExtCamera
            yield sprintf "ext_stats=%b" req.ExtParams.ExtStats
            yield sprintf "ext_collection=%b" req.ExtCollection
            yield req.Deviationids
                |> Seq.map (fun o -> o.ToString())
                |> String.concat ","
                |> sprintf "deviationids[]=%s"
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/deviation/metadata?%s"
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        return json |> MetadataResponse.Parse
    }

    let ExecuteAsync token req = AsyncExecute token req |> iop.thenTo (fun o -> o.Metadata) |> iop.thenMap Metadata |> Async.StartAsTask
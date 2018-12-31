﻿namespace DeviantArtFs.Stash

open DeviantArtFs
open FSharp.Data

type internal DeltaResponse = JsonProvider<"""[{
    "cursor": "cursor1",
    "has_more": true,
    "next_offset": 42,
    "reset": false,
    "entries": [
        {
            "stackid": 1555555555555555,
            "metadata": {},
            "position": 40
        },
        {
            "itemid": 5555555555555555,
            "stackid": 88888888888880,
            "metadata": {},
            "position": 0
        },
        {
            "itemid": 7777777777777777,
            "stackid": null,
            "metadata": null
        }
    ]
}, {
    "cursor": "cursor2",
    "has_more": false,
    "next_offset": null,
    "reset": false,
    "entries": []
}]""", SampleIsList=true>

type DeltaResultEntry =
    {
        Itemid: int64 option
        Stackid: int64 option
        Metadata: StashMetadata.Root option
        Position: int option
    }
    with
        member this.GetItemid() = this.Itemid |> Option.toNullable
        member this.GetStackid() = this.Stackid |> Option.toNullable
        member this.GetMetadata() = this.Metadata |> Option.map (fun j -> j.JsonValue.ToString()) |> Option.toObj
        member this.GetPosition() = this.Position |> Option.toNullable
        interface IDeltaEntry with
            member this.Itemid = this.GetItemid()
            member this.Stackid = this.GetStackid()
            member this.Metadata = this.GetMetadata()
            member this.Position = this.GetPosition()

type DeltaResult = {
    Cursor: string
    HasMore: bool
    NextOffset: System.Nullable<int>
    Reset: bool
    Entries: seq<DeltaResultEntry>
}

type DeltaRequest() = 
    member val Cursor = null with get, set
    member val Offset = 0 with get, set
    member val Limit = 120 with get, set
    member val ExtParams = new ExtParams() with get, set

module Delta =
    let AsyncExecute token (req: DeltaRequest) = async {
        let query = seq {
            match Option.ofObj req.Cursor with
            | Some s -> yield sprintf "cursor=%s" (dafs.urlEncode s)
            | None -> ()
            yield sprintf "offset=%d" req.Offset
            yield sprintf "limit=%d" req.Limit
            yield sprintf "ext_submission=%b" req.ExtParams.ExtSubmission
            yield sprintf "ext_camera=%b" req.ExtParams.ExtCamera
            yield sprintf "ext_stats=%b" req.ExtParams.ExtStats
        }
        let req =
            query
            |> String.concat "&"
            |> sprintf "https://www.deviantart.com/api/v1/oauth2/stash/delta?%s"
            |> dafs.createRequest token
        let! json = dafs.asyncRead req
        let resp = DeltaResponse.Parse json
        return {
            Cursor = resp.Cursor
            HasMore = resp.HasMore
            NextOffset = resp.NextOffset |> Option.toNullable
            Reset = resp.Reset
            Entries = seq {
                for e in resp.Entries do
                    yield {
                        Itemid = e.Itemid
                        Stackid = e.Stackid
                        Metadata = e.Metadata |> Option.map (fun j -> j.JsonValue.ToString() |> StashMetadata.Parse)
                        Position = e.Position
                    }
            }
        }
    }

    let ExecuteAsync token req = AsyncExecute token req |> Async.StartAsTask
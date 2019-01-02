﻿namespace DeviantArtFs

open System
open System.Net
open FSharp.Data

type internal DeviantArtBaseResponse = JsonProvider<"""{"status":"error"}""">

type internal DeviantArtErrorResponse = JsonProvider<"""{"error":"invalid_request","error_description":"Must provide an access_token to access this resource.","status":"error"}""">

type internal SuccessOrErrorResponse = JsonProvider<"""[
{ "success": false, "error_description": "str" },
{ "success": true }
]""", SampleIsList=true>

type internal GenericListResponse = JsonProvider<"""[
{ "has_more": true, "next_offset": 2, "estimated_total": 7, "results": [] },
{ "has_more": false, "next_offset": null, "results": [] }
]""", SampleIsList=true>

type internal ListOnlyResponse = JsonProvider<"""{ "results": [] }""">

type IDeviantArtAccessToken =
    abstract member AccessToken: string with get

type IDeviantArtRefreshToken =
    inherit IDeviantArtAccessToken
    abstract member ExpiresAt: DateTimeOffset with get
    abstract member RefreshToken: string with get

type DeviantArtException(resp: WebResponse, body: DeviantArtErrorResponse.Root) =
    inherit Exception(body.ErrorDescription)

    member __.ResponseBody = body
    member __.StatusCode =
        match resp with
        | :? HttpWebResponse as h -> Nullable h.StatusCode
        | _ -> Nullable()

type ExtParams =
    struct
        val ExtSubmission: bool
        val ExtCamera: bool
        val ExtStats: bool
    end

[<AllowNullLiteral>]
type IDeviantArtUser =
    abstract member Userid: Guid
    abstract member Username: string
    abstract member Usericon: string
    abstract member Type: string

type UserResult = {
    Userid: Guid
    Username: string
    Usericon: string
    Type: string
} with
    interface IDeviantArtUser with
        member this.Userid = this.Userid
        member this.Username = this.Username
        member this.Usericon = this.Usericon
        member this.Type = this.Type

type DeviantArtPagedResult<'a> = {
    HasMore: bool
    NextOffset: int option
    Results: seq<'a>
} with
    member this.GetNextOffset() = this.NextOffset |> Option.toNullable

type DeviantArtPagedSearchResult<'a> = {
    HasMore: bool
    NextOffset: int option
    EstimatedTotal: int option
    Results: seq<'a>
} with
    member this.GetNextOffset() = this.NextOffset |> Option.toNullable
    member this.GetEstimatedTotal() = this.EstimatedTotal |> Option.toNullable

type IDeltaEntry =
    abstract member Itemid: Nullable<int64>
    abstract member Stackid: Nullable<int64>
    abstract member Metadata: string
    abstract member Position: Nullable<int>
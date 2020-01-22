﻿namespace DeviantArtFs

open FSharp.Json

/// A single page of results from a DeviantArt API endpoint.
type IBclDeviantArtFolderPagedResult =
    inherit IBclDeviantArtPagedResult<IBclDeviation>

    /// The name, if provided.
    abstract member Name: string

/// A single page of results from a DeviantArt API endpoint.
type DeviantArtFolderPagedResult = {
    has_more: bool
    next_offset: int option
    name: string option
    results: Deviation list
} with
    static member Parse json = Json.deserialize<DeviantArtFolderPagedResult> json
    interface IBclDeviantArtFolderPagedResult with
        member this.HasMore = this.has_more
        member this.NextOffset = this.next_offset |> Option.toNullable
        member this.Name = this.name |> Option.toObj
        member this.Results = this.results |> Seq.map (fun o -> o :> IBclDeviation)
    interface IResultPage<int, Deviation> with
        member this.HasMore = this.has_more
        member this.Cursor = this.next_offset |> Option.defaultValue 0
        member this.Items = this.results |> Seq.ofList
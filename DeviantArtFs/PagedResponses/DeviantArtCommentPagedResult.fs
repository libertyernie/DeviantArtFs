﻿namespace DeviantArtFs

/// A single page of results from a DeviantArt API endpoint that fetches
/// comments.
type DeviantArtCommentPagedResult = {
    has_more: bool
    next_offset: int option
    has_less: bool
    prev_offset: int option
    total: int option
    thread: DeviantArtComment list
} with
    interface IDeviantArtResultPage<ParameterTypes.PagingOffset, DeviantArtComment> with
        member this.HasMore = this.has_more
        member this.Cursor = this.next_offset |> Option.map ParameterTypes.PagingOffset |> Option.defaultValue ParameterTypes.FromStart
        member this.Items = this.thread |> Seq.ofList
﻿namespace DeviantArtFs

open System
open System.Net
open FSharp.Control

module internal Dafs =
    /// Throws an exception if there was an error, or does nothing if there was not.
    /// This is used to catch errors that come back with an HTTP 2xx status code (if there even are any).
    let assertSuccess (resp: DeviantArtSuccessOrErrorResponse) =
        match (resp.success, resp.error_description) with
        | (true, None) -> ()
        | _ -> failwithf "%s" (resp.error_description |> Option.defaultValue "An unknown error occurred.")

    /// URL-encodes a string.
    let urlEncode = WebUtility.UrlEncode

    /// Creates a DeviantArtRequest object, given a token object (possibly with common parameters such as user expansion parameters) and a URL.
    let createRequest (token: IDeviantArtAccessToken) (url: string) =
        let full_url =
            match token with
            | :? IDeviantArtAccessTokenWithCommonParameters as p ->
                let expand = seq {
                    if p.Expand.HasFlag(DeviantArtObjectExpansion.UserDetails) then
                        yield sprintf "user.details"
                    if p.Expand.HasFlag(DeviantArtObjectExpansion.UserGeo) then
                        yield sprintf "user.geo"
                    if p.Expand.HasFlag(DeviantArtObjectExpansion.UserProfile) then
                        yield sprintf "user.profile"
                    if p.Expand.HasFlag(DeviantArtObjectExpansion.UserStats) then
                        yield sprintf "user.stats"
                    if p.Expand.HasFlag(DeviantArtObjectExpansion.UserWatch) then
                        yield sprintf "user.watch"
                }
                let query = seq {
                    yield sprintf "mature_content=%b" p.MatureContent
                    if p.Expand <> DeviantArtObjectExpansion.None then
                        yield expand |> String.concat "," |> sprintf "expand=%s"
                }
                String.concat "" (seq {
                    yield url
                    if not (url.Contains("?")) then
                        yield "?"
                    yield query |> String.concat "&"
                })
            | _ -> url
        new DeviantArtRequest(token, full_url)

    /// Executes a DeviantArtRequest and gets the response body.
    let asyncRead (req: DeviantArtRequest) = req.AsyncReadJson()
    
    /// Converts a paged function with offset and limit parameters to one that requests the maximum page size each time.
    let getMax (f: IDeviantArtAccessToken -> IDeviantArtPagingParams -> 'a) (token: IDeviantArtAccessToken) (offset: int) =
        new DeviantArtPagingParams(Offset = offset, Limit = Nullable Int32.MaxValue)
        |> f token

    /// Converts a paged function that takes a "cursor" as one of its parameters into an AsyncSeq.
    let toAsyncSeq (initial_cursor: 'cursor) (req: 'req) (f: 'cursor -> 'req -> Async<'b> when 'b :> IResultPage<'cursor, 'item>) = asyncSeq {
        let mutable cursor = initial_cursor
        let mutable has_more = true
        while has_more do
            let! resp = f cursor req
            for r in resp.Items do
                yield r
            cursor <- resp.Cursor
            has_more <- resp.HasMore
    }
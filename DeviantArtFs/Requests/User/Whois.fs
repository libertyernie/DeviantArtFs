﻿namespace DeviantArtFs.Requests.User

open DeviantArtFs
open System.IO

module Whois =
    let AsyncExecute token usernames = async {
        let query = seq {
            for u in usernames do
                yield u |> Dafs.urlEncode |> sprintf "usernames[]=%s"
        }
        let req = Dafs.createRequest token DeviantArtCommonParams.Default "https://www.deviantart.com/api/v1/oauth2/user/whois"
        req.Method <- "POST"
        req.ContentType <- "application/x-www-form-urlencoded"
        req.RequestBodyText <- String.concat "&" query
        let! json = Dafs.asyncRead req
        return DeviantArtListOnlyResponse<DeviantArtUser>.ParseList json
    }

    let ExecuteAsync token usernames = AsyncExecute token usernames |> Async.StartAsTask
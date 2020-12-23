﻿namespace DeviantArtFs.Api.Deviation

open DeviantArtFs
open System

module Download =
    let AsyncExecute token (deviationid: Guid) = async {
        let req = sprintf "https://www.deviantart.com/api/v1/oauth2/deviation/download/%O" deviationid |> Dafs.createRequest token DeviantArtCommonParams.Default
        let! json = Dafs.asyncRead req
        return DeviationDownload.Parse json
    }

    let ExecuteAsync token deviationid = AsyncExecute token deviationid |> Async.StartAsTask
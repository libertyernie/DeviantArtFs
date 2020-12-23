﻿namespace DeviantArtFs.Requests.Stash

open DeviantArtFs

module PublishUserdata =
    let AsyncExecute token = async {
        let req = Dafs.createRequest token DeviantArtCommonParams.Default "https://www.deviantart.com/api/v1/oauth2/stash/publish/userdata"

        let! json = Dafs.asyncRead req
        return StashPublishUserdataResult.Parse json
    }

    let ExecuteAsync token = AsyncExecute token |> Async.StartAsTask
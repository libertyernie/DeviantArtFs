﻿namespace DeviantArtFs

open System
open FSharp.Json

type DeviantArtCollectionFolder = {
    folderid: Guid
    name: string
    size: int option
    deviations: Deviation list option
} with
    static member Parse json = Json.deserialize<DeviantArtCollectionFolder> json
    member this.GetSize() = OptUtils.toNullable this.size
    member this.GetDeviations() = OptUtils.listDefault this.deviations
﻿namespace DeviantArtFs

open System

type IBclDeviantArtNote =
    abstract member Noteid: Guid
    abstract member Ts: DateTimeOffset
    abstract member Unread: bool
    abstract member Starred: bool
    abstract member Sent: bool
    abstract member Subject: string
    abstract member Preview: string
    abstract member Body: string
    abstract member User: IBclDeviantArtUser
    abstract member Recipients: seq<IBclDeviantArtUser>

type DeviantArtNote = {
    noteid: Guid
    ts: DateTimeOffset
    unread: bool
    starred: bool
    sent: bool
    subject: string
    preview: string
    body: string
    user: DeviantArtUser
    recipients: DeviantArtUser[]
} with
    interface IBclDeviantArtNote with
        member this.Body = this.body
        member this.Noteid = this.noteid
        member this.Preview = this.preview
        member this.Recipients = this.recipients |> Seq.map (fun o -> o :> IBclDeviantArtUser)
        member this.Sent = this.sent
        member this.Starred = this.starred
        member this.Subject = this.subject
        member this.Ts = this.ts
        member this.Unread = this.unread
        member this.User = this.user :> IBclDeviantArtUser
﻿namespace DeviantArtFs

module internal iop =
    let thenTo f a = async {
        let! o = a
        return f o
    }

    let thenMap f a = async {
        let! o = a
        return Seq.map f o
    }

    let thenMapResult f a = async {
        let! o = a
        let r = o :> IBclDeviantArtPagedResult<'a>
        return {
            new IBclDeviantArtPagedResult<'b> with
                member __.HasMore = r.HasMore
                member __.NextOffset = r.NextOffset
                member __.HasLess = r.HasLess
                member __.PrevOffset = r.PrevOffset
                member __.EstimatedTotal = r.EstimatedTotal
                member __.Name = r.Name
                member __.Results = Seq.map f r.Results
        }
    }

    let thenCastResult a = thenMapResult id a

    let thenMapFeedResult f a = async {
        let! o = a
        let r = o :> IBclDeviantArtFeedPagedResult<'a>
        return {
            new IBclDeviantArtFeedPagedResult<'b> with
                member __.HasMore = r.HasMore
                member __.Cursor = r.Cursor
                member __.Items = Seq.map f r.Items
        }
    }

    let thenCastFeedResult a = thenMapFeedResult id a
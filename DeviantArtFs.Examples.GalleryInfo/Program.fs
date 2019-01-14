﻿open System
open System.IO
open DeviantArtFs

// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

let create_token_obj str = { new IDeviantArtAccessToken with member __.AccessToken = str }

let get_token =
    let token_file = "token.txt"
    let token_string =
        if File.Exists token_file
        then File.ReadAllText token_file
        else ""

    let valid = create_token_obj token_string |> DeviantArtFs.Requests.Util.Placebo.AsyncIsValid |> Async.RunSynchronously
    if valid then
        token_string
    else
        printf "Please enter the client ID (e.g. 1234): "
        let client_id = System.Console.ReadLine() |> Int32.Parse

        printf "Please enter the redirect URL (default: https://www.example.com): "
        let url1 = System.Console.ReadLine()
        let url2 =
            match url1 with
            | "" -> "https://www.example.com"
            | s -> s

        use form = new DeviantArtFs.WinForms.DeviantArtImplicitGrantForm(client_id, new Uri(url2), ["browse"; "user"; "stash"; "publish"; "user.manage"])
        if form.ShowDialog() <> System.Windows.Forms.DialogResult.OK then
            failwithf "Login cancelled"
        else
            File.WriteAllText("token.txt", form.AccessToken)
            form.AccessToken

let page offset limit = new PagingParams(Offset = offset, Limit = Nullable limit)

let sandbox token_string = async {
    let token = create_token_obj token_string

    printf "Enter a username (leave blank to see your own submissions): "
    let read = Console.ReadLine()

    let! me = DeviantArtFs.Requests.User.Whoami.AsyncExecute token

    let username =
        match read with
        | "" -> me.Username
        | s -> s

    let! statuses = DeviantArtFs.Requests.User.StatusesList.AsyncExecute token (page 0 1) username
    let status = Seq.tryHead statuses.Results
    match status with
    | Some s -> 
        printfn "Most recent status: %s (%O)" s.Body s.Ts
        printfn ""
    | None -> ()

    let! stash = DeviantArtFs.Requests.Stash.Delta.GetAllAsListAsync token (new ExtParams()) |> Async.AwaitTask
    for s in stash do
        printfn "%s" s.Metadata.Title

    let list = new ResizeArray<Deviation>()
    let mutable offset = 0
    let mutable more = true
    while more do
        let req = new DeviantArtFs.Requests.Gallery.GalleryAllViewRequest()
        let paging = new PagingParams(Offset = 0, Limit = Nullable 24)
        let! (resp: DeviantArtPagedResult<Deviation>) = DeviantArtFs.Requests.Gallery.GalleryAllView.AsyncExecute token paging req
        list.AddRange(resp.Results)
        offset <- resp.NextOffset |> Option.defaultValue 0
        more <- resp.HasMore

    return ()
}

[<EntryPoint>]
[<STAThread>]
let main _ =
    let token_string = get_token
    sandbox token_string |> Async.RunSynchronously
    0
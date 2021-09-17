module GamesFaix.MtgTools.Designer.MtgDesign.Auth

open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Model
open System.Net.Http
open System.Collections.Generic

let private loginUrl = "https://mtg.design/login"
let private token = "8nHrnmDXMDPZiXZXmfgeTkPnZJDVvMGWqdPTKMDi" // TODO: Figure out what this is

let private getCookieFromResponse (name: string) (response: HttpResponseMessage): string option =
    match response.Headers.TryGetValues("Set-Cookie") with
    | true, cookies ->
        match cookies |> Seq.tryFind (fun c -> c.StartsWith name) with
        | Some c ->
            c.Substring(name.Length + 1) |> Some // +1 for = symbol
        | _ -> None
    | _ -> None

let private getCookiesFromLoginPage (http: HttpClient) : unit Async =
    async {
        let! response = http.GetAsync loginUrl |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        return ()
    }

let private sendLoginRequest (http: HttpClient) (creds: Credentials) : unit Async =
    async {
        use request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
        //request.Headers.Add("Host", "mtg.design")
        //request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0")
        //request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
        //request.Headers.Add("Accept-Language", "en-US,en;q=0.5")
        //request.Headers.Add("Accept-Encoding", "gzip, deflate, br")
        request.Content <- new FormUrlEncodedContent([
            KeyValuePair("_token", token)
            KeyValuePair("email", creds.Email)
            KeyValuePair("password", creds.Password)
            KeyValuePair("remember", "on")
        ])
        //request.Headers.Add("Origin", "https://mtg.design")
        //request.Headers.Add("Connection", "keep-alive")
        //request.Headers.Add("Referer", loginUrl)
        //request.Headers.Add("Upgrade-Insecure-Requests", "1")
        //request.Headers.Add("Sec-Fetch-Dest", "document")
        //request.Headers.Add("Sec-Fetch-Mode", "navigate")
        //request.Headers.Add("Sec-Fetch-Site", "same-origin")
        //request.Headers.Add("Sec-Fetch-User", "?1")

        let! response = http.SendAsync request |> Async.AwaitTask

        let! text = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        let rememberWeb = response |> getCookieFromResponse "remember_web"

        return ()
    }

let login (ctx: Context) : unit Async =
    async {
        let! creds = FileSystem.loadCredentials ctx.RootDir
        if creds.IsNone then failwith "Could not load credentials"

        do! getCookiesFromLoginPage ctx.Http
        let! _ = sendLoginRequest ctx.Http creds.Value


        return failwith "Not implemented"
    }

module GamesFaix.MtgTools.Designer.Auth

open System
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Shared

let private loginUrl = "https://mtg.design/login"
let private homeUrl = "https://mtg.design/home"
let private timeout = TimeSpan.FromSeconds 30.0

type Credentials = {
    Email : string
    Password : string
}

type Cookie = {
    Name : string
    Value : string
}

let private saveCredentialsFile (creds: Credentials) (workspace: Workspace.WorkspaceDirectory) =
    FileSystem.saveToJson creds workspace.Credentials

let private loadCredentialsFile (workspace: Workspace.WorkspaceDirectory) =
    FileSystem.loadFromJson<Credentials> workspace.Credentials

let private saveCookieFile (cookie: Cookie) (workspace: Workspace.WorkspaceDirectory) =
    FileSystem.saveToJson cookie workspace.Cookie

let loadCookieFile (workspace: Workspace.WorkspaceDirectory) =
    FileSystem.loadFromJson<Cookie> workspace.Cookie

module private Browser =
    let private getDriver () =
        let options = ChromeOptions()
        options.AddArgument("--headless")
        let driver = new ChromeDriver(options)
        driver

    let login (credentials: Credentials) =
        use driver = getDriver ()
        driver.Navigate().GoToUrl loginUrl

        let emailInput =
            WebDriverWait(driver, timeout)
                .Until(fun d -> d.FindElement(By.CssSelector "input[name=email]" ))
        let passInput = driver.FindElement(By.CssSelector "input[name=password]")
        let submitBtn = driver.FindElement(By.CssSelector "button[type=submit]")

        emailInput.SendKeys credentials.Email
        passInput.SendKeys credentials.Password
        submitBtn.Click()

        WebDriverWait(driver, timeout)
            .Until(fun d -> d.Url = homeUrl)
            |> ignore

        let cookie =
            driver.Manage().Cookies.AllCookies
            |> Seq.tryFind (fun c -> c.Name.StartsWith "remember_web_")

        match cookie with
        | None -> Error "Failed to login"
        | Some c -> Ok { Name = c.Name; Value = c.Value }

    let testCookie (cookie: Cookie) : Result<unit, string> =
        let driver = getDriver ()
        driver.Manage().Cookies.AddCookie(Cookie(cookie.Name, cookie.Value))
        driver.Navigate().GoToUrl homeUrl
        let loaded =
            WebDriverWait(driver, timeout)
                .Until(fun d ->
                    d.FindElement(By.CssSelector "h2")
                     .Text.StartsWith "Hello, "
                )

        if loaded then Ok ()
        else Error "Failed to load home page with cookie"

let private ensureCredentials (credentials: Credentials option) workspace =
    async {
        match credentials with
        | Some creds -> return Ok creds
        | None ->
            match! loadCredentialsFile workspace with
            | Some creds -> return Ok creds
            | None -> return Error $"No saved credentials in workspace {workspace.Path}"
    }

let login (credentials: Credentials option) (saveCredentials: bool) workspace =
    async {
        match! ensureCredentials credentials workspace with
        | Error err -> return Error err
        | Ok creds ->
            match Browser.login creds with
            | Error err -> return Error err
            | Ok cookie ->
                if saveCredentials && credentials.IsSome then
                    do! saveCredentialsFile creds workspace
                do! saveCookieFile cookie workspace
                return Ok ()
    }

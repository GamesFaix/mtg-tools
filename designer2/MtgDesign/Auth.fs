module GamesFaix.MtgTools.Designer.MtgDesign.Auth

open System
open System.Collections.Generic
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Model

let private loginUrl = "https://mtg.design/login"
let private homeUrl = "https://mtg.design/home"
let private timeout = TimeSpan.FromSeconds 30.0

let private login (ctx: Context) : KeyValuePair<string, string> Async =
    async {
        let! creds = FileSystem.loadCredentials ctx.Workspace
        if creds.IsNone then failwith "Could not load credentials"

        use driver = new ChromeDriver()
        driver.Navigate().GoToUrl loginUrl

        let emailInput =
            WebDriverWait(driver, timeout)
                .Until(fun d -> d.FindElement(By.CssSelector "input[name=email]" ))
        let passInput = driver.FindElement(By.CssSelector "input[name=password]")
        let submitBtn = driver.FindElement(By.CssSelector "button[type=submit]")

        emailInput.SendKeys creds.Value.Email
        passInput.SendKeys creds.Value.Password
        submitBtn.Click()

        WebDriverWait(driver, timeout)
            .Until(fun d -> d.Url = homeUrl)
            |> ignore

        let cookie =
            driver.Manage().Cookies.AllCookies
            |> Seq.tryFind (fun c -> c.Name.StartsWith "remember_web_")

        match cookie with
        | None -> return failwith "Failed to login"
        | Some c -> return KeyValuePair(c.Name, c.Value)
    }

let ensureValidCookie (ctx: Context) : unit Async =
    async {
        let! previousCookie = FileSystem.loadCookie ctx.Workspace

        match previousCookie with
        | None ->
            // If no cookie, login and save cookie
            let! newCookie = login ctx
            do! FileSystem.saveCookie ctx.Workspace newCookie
            return ()
        | Some c ->
            // Test cookie and refresh if required
            let driver = new ChromeDriver()
            driver.Manage().Cookies.AddCookie(Cookie(c.Key, c.Value))
            driver.Navigate().GoToUrl homeUrl

            let loaded =
                WebDriverWait(driver, timeout)
                    .Until(fun d -> d.FindElement(By.CssSelector "h2")
                                     .Text.StartsWith "Hello, ")

            if not loaded then
                let! newCookie = login ctx
                do! FileSystem.saveCookie ctx.Workspace newCookie

            return ()
    }
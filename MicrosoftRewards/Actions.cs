using System.Web;
using Colorify;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static class Actions
{
    public static void Login(ChromeDriver driver, string login, string password)
    {
        driver.Navigate().GoToUrl(Program.BaseUrl);

        Utils.SmallSleep();

        var emailElement = driver.FindElement(By.Id("i0116"));
        Utils.TypeText(emailElement, login);

        Utils.SmallSleep();

        var enterEmailButton = driver.FindElement(By.Id("idSIButton9"));
        Utils.Click(driver, enterEmailButton);

        Utils.SmallSleep();

        try
        {
            driver.FindElement(By.Id("oneTimeCodeDescription"));
            Utils.LargeSleep();
            var emailCode = Utils.GetEmailCode(login);
            var codeElement = driver.FindElement(By.Id("idTxtBx_OTC_Password"));
            Utils.TypeText(codeElement, emailCode ?? string.Empty);
            var enterCodeButton = driver.FindElement(By.Id("primaryButton"));
            Utils.Click(driver, enterCodeButton);
            Utils.SmallSleep();
            var acceptCodeButton = driver.FindElement(By.Id("acceptButton"));
            Utils.Click(driver, acceptCodeButton);
            return;
        }
        catch
        {
        }
        
        var passwordElement = driver.FindElement(By.Id("i0118"));
        Utils.TypeText(passwordElement, password);

        Utils.SmallSleep();

        var enterPasswordButton = driver.FindElement(By.Id("idSIButton9"));
        Utils.Click(driver, enterPasswordButton);

        Utils.SmallSleep();

        var acceptButton = driver.FindElement(By.Id("acceptButton"));
        Utils.Click(driver, acceptButton);

        Utils.SmallSleep();
    }

    public static void CompleteDailyTasks(ChromeDriver driver)
    {
        driver.Navigate().GoToUrl(Program.BaseUrl);
        dynamic dashboard = Utils.GetDashboard(driver);
        var dailyTasks = dashboard["dailySetPromotions"];
        var todayDate = DateTime.Now.ToString("MM/dd/yyyy").Replace(".", "/");

        var todayDailyTasks = dailyTasks[todayDate];

        foreach (var todayDailyTask in todayDailyTasks)
        {
            try
            {
                if (todayDailyTask["complete"]) continue;
                var cardId = int.Parse(todayDailyTask["offerId"].ToString()
                    .Substring(todayDailyTask["offerId"].Length - 1));
                DailyTasks.OpenDailySetActivity(driver, cardId);

                if (todayDailyTask["promotionType"] == "urlreward")
                {
                    Utils.LargeSleep();
                    Utils.CloseCurrentTab(driver);
                }

                if (todayDailyTask["promotionType"] == "quiz")
                {
                    if (todayDailyTask["pointProgressMax"] == 50 && todayDailyTask["pointProgress"] == 0)
                    {
                        DailyTasks.CompleteThisOrThat(driver);
                    }
                    else if ((todayDailyTask["pointProgressMax"] == 40 ||
                              todayDailyTask["pointProgressMax"] == 30) &&
                             todayDailyTask["pointProgress"] == 0)
                    {
                        DailyTasks.CompleteQuiz(driver);
                    }
                    else if (todayDailyTask["pointProgressMax"] == 10 && todayDailyTask["pointProgress"] == 0)
                    {
                        var destinationUri = new Uri(todayDailyTask["destinationUrl"]);
                        var queryParameters = HttpUtility.ParseQueryString(destinationUri.Query);
                        var searchUrl = HttpUtility.UrlDecode(queryParameters["ru"]);

                        var searchUri = new Uri(searchUrl ?? string.Empty);
                        var searchUrlQueries = HttpUtility.ParseQueryString(searchUri.Query);

                        var filters = new Dictionary<string, string>();

                        var filtersParam = searchUrlQueries["filters"];
                        if (filtersParam != null)
                        {
                            var filterElements = filtersParam.Split(' ');
                            foreach (var filterElement in filterElements)
                            {
                                var filterParts = filterElement.Split(':', 2);
                                if (filterParts.Length == 2)
                                {
                                    filters[filterParts[0]] = filterParts[1];
                                }
                            }
                        }

                        if (filters.ContainsKey("PollScenarioId"))
                        {
                            Utils.CompleteSurvey(driver);
                        }
                        else
                        {
                            try
                            {
                                DailyTasks.CompleteAbc(driver);
                            }
                            catch
                            {
                                DailyTasks.CompleteQuiz(driver);
                            }
                        }
                    }
                }
            }
            catch
            {
                Utils.ResetTabs(driver);
            }
        }
    }

    public static void CompletePunchCards(ChromeDriver driver)
    {
        driver.Navigate().GoToUrl(Program.BaseUrl);
        PunchCards.CompletePromotionalItems(driver);
        dynamic dashboard = Utils.GetDashboard(driver);
        var punchCards = dashboard["punchCards"];

        foreach (var punchCard in punchCards)
        {
            try
            {
                if (punchCard["parentPromotion"] != null &&
                    punchCard["childPromotions"] != null &&
                    !(bool)punchCard["parentPromotion"]["complete"] &&
                    (int)punchCard["parentPromotion"]["pointProgressMax"] != 0)
                {
                    PunchCards.CompletePunchCard(driver,
                        punchCard["parentPromotion"]["attributes"]["destination"],
                        punchCard["childPromotions"]
                    );
                }
            }
            catch
            {
                Utils.ResetTabs(driver);
            }
        }

        Utils.SmallSleep();
        driver.Navigate().GoToUrl(Program.BaseUrl);
        Utils.SmallSleep();
    }

    public static void CompleteMorePromotions(ChromeDriver driver, Format colorify)
    {
        driver.Navigate().GoToUrl(Program.BaseUrl);
        dynamic dashboard = Utils.GetDashboard(driver);
        var morePromotions = dashboard["morePromotions"];
        foreach (var promotion in morePromotions)
        {
            try
            {
                string promotionTitle = promotion["title"].Replace("\u200b", "").Replace("\xa0", " ");

                if (promotion["complete"] != false || promotion["pointProgressMax"] == 0)
                {
                    continue;
                }

                MorePromotions.OpenMorePromotionsActivity(driver, morePromotions.IndexOf(promotion));
                driver.ExecuteScript("window.scrollTo(0, 1080)");

                try
                {
                    var searchbar = driver.FindElement(By.Id("sb_form_q"));
                    Utils.Click(driver, searchbar);
                }
                catch
                {
                }

                if (promotionTitle.Contains("Search the lyrics of a song"))
                {
                    MorePromotions.PerformSearch(driver, "black sabbath supernaut lyrics");
                }
                else if (promotionTitle.Contains("Translate anything"))
                {
                    MorePromotions.PerformSearch(driver, "translate pencil sharpener to spanish");
                }
                else if (promotionTitle.Contains("Let's watch that movie again!"))
                {
                    MorePromotions.PerformSearch(driver, "aliens movie");
                }
                else if (promotionTitle.Contains("Discover open job roles"))
                {
                    MorePromotions.PerformSearch(driver, "walmart open job roles");
                }
                else if (promotionTitle.Contains("Plan a quick getaway"))
                {
                    MorePromotions.PerformSearch(driver, "flights nyc to paris");
                }
                else if (promotionTitle.Contains("You can track your package"))
                {
                    MorePromotions.PerformSearch(driver, "usps tracking");
                }
                else if (promotionTitle.Contains("Find somewhere new to explore"))
                {
                    MorePromotions.PerformSearch(driver, "directions to new york");
                }
                else if (promotionTitle.Contains("Too tired to cook tonight?"))
                {
                    MorePromotions.PerformSearch(driver, "Pizza Hut near me");
                }
                else if (promotionTitle.Contains("Quickly convert your money"))
                {
                    MorePromotions.PerformSearch(driver, "convert 374 usd to yen");
                }
                else if (promotionTitle.Contains("Learn to cook a new recipe"))
                {
                    MorePromotions.PerformSearch(driver, "how cook pierogi");
                }
                else if (promotionTitle.Contains("Find places to stay"))
                {
                    MorePromotions.PerformSearch(driver, "hotels rome italy");
                }
                else if (promotionTitle.Contains("How's the economy?"))
                {
                    MorePromotions.PerformSearch(driver, "sp 500");
                }
                else if (promotionTitle.Contains("Who won?"))
                {
                    MorePromotions.PerformSearch(driver, "braves score");
                }
                else if (promotionTitle.Contains("Gaming time"))
                {
                    MorePromotions.PerformSearch(driver, "vampire survivors video game");
                }
                else if (promotionTitle.Contains("Expand your vocabulary"))
                {
                    MorePromotions.PerformSearch(driver, "definition definition");
                }
                else if (promotionTitle.Contains("What time is it?"))
                {
                    MorePromotions.PerformSearch(driver, "china time");
                }
                else if (promotion["promotionType"] == "urlreward")
                {
                    Utils.CompleteSearch(driver);
                }
                else if (promotion["promotionType"] == "quiz")
                {
                    MorePromotions.CompleteQuizBasedOnPoints(driver, promotion["pointProgressMax"]);
                }
                else
                {
                    Utils.CompleteSearch(driver);
                }

                driver.ExecuteScript("window.scrollTo(0, 1080)");
                Utils.LargeSleep();

                Utils.ResetTabs(driver);
                Utils.SmallSleep();
            }
            catch
            {
                Utils.ResetTabs(driver);
            }
        }

        var incompletePromotions = new List<(string, string)>();
        foreach (var promotion in dashboard["morePromotions"])
        {
            if (promotion["pointProgress"] < promotion["pointProgressMax"])
            {
                incompletePromotions.Add((promotion["title"], promotion["promotionType"]));
            }
        }

        if (incompletePromotions.Count > 0)
        {
            colorify.WriteLine($"Incomplete promotions(s) {incompletePromotions}", Colors.txtWarning);
        }
    }

    public static void CompleteBrowserBingSearches(ChromeDriver driver, ILogger logger, Format colorify)
    {
        var remainingSearches = BingSearches.GetRemainingSearches(driver, false);
        logger.LogInformation("[BING] Remaining searches={@RemainingSearches}", remainingSearches.Desktop);
        colorify.WriteLine($"[BING] Remaining searches: {remainingSearches.Desktop}", Colors.txtInfo);
        if (remainingSearches.Desktop <= 0) return;
        var trends = BingSearches.GetGoogleTrends(remainingSearches.Desktop);
        // var random = new Random();
        // trends = trends.OrderBy(x => random.Next()).ToList();
        driver.Navigate().GoToUrl(Program.SearchUrl);
        Utils.SmallSleep();
        try
        {
            var loginButton = driver.FindElement(By.Id("id_s"));
            Utils.Click(driver, loginButton);
            Utils.SmallSleep();
            var popupLoginButton = driver.FindElement(By.ClassName("id_text_signin"));
            Utils.Click(driver, popupLoginButton);
            Utils.SmallSleep();
        }
        catch
        {
        }
        BingSearches.BingSearch(driver, trends);
    }


    public static async void BingLogin(ChromeDriver driver)
    {
        var cookies = driver.Manage().Cookies.AllCookies;

        var cookieContainer = new System.Net.CookieContainer();
        foreach (var cookie in cookies)
        {
            cookieContainer.Add(new Uri("https://www.bing.com"), new System.Net.Cookie(cookie.Name, cookie.Value));
        }

        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer
        };

        using (var clientWithCookies = new HttpClient(handler))
        {
            var response = await clientWithCookies.GetAsync("https://www.bing.com/rewards/panelflyout/getuserinfo");

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            // Console.WriteLine(responseBody);
        }
    }
}
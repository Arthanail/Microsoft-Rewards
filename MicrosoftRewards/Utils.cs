using System.Text;
using System.Text.RegularExpressions;
using MimeKit;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static partial class Utils
{
    public static void TypeText(IWebElement element, string text)
    {
        foreach (var character in text)
        {
            element.SendKeys(character.ToString());
            Task.Delay(100 + new Random().Next(0, 100));
        }
    }

    public static void SmallSleep()
    {
        var time = new Random().Next(2, 5);
        Thread.Sleep(TimeSpan.FromSeconds(time));
    }

    public static void LargeSleep()
    {
        var time = new Random().Next(8, 12);
        Thread.Sleep(TimeSpan.FromSeconds(time));
    }

    public static object GetDashboard(ChromeDriver driver)
    {
        return driver.ExecuteScript("return dashboard");
    }

    public static void CompleteSurvey(ChromeDriver driver)
    {
        var btButton = driver.FindElement(By.Id($"btoption{new Random().Next(0, 1)}"));
        Click(driver, btButton);
        LargeSleep();
        CloseCurrentTab(driver);
    }

    public static void CloseCurrentTab(ChromeDriver driver)
    {
        driver.Close();
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
        driver.SwitchTo().Window(driver.WindowHandles[0]);
        Thread.Sleep(TimeSpan.FromSeconds(0.5));
    }

    public static void SwitchToNewTab(ChromeDriver driver, int timeToWait = 0)
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeToWait));
        driver.SwitchTo().Window(driver.WindowHandles[1]);
    }

    public static void VisitNewTab(ChromeDriver driver, int timeToWait = 0)
    {
        SwitchToNewTab(driver, timeToWait);
        CloseCurrentTab(driver);
    }

    public static void CompleteSearch(ChromeDriver driver)
    {
        LargeSleep();
        CloseCurrentTab(driver);
    }

    public static void ResetTabs(ChromeDriver driver)
    {
        var currentWindowHandle = driver.CurrentWindowHandle;

        foreach (var handle in driver.WindowHandles)
        {
            if (handle == currentWindowHandle) continue;
            driver.SwitchTo().Window(handle);
            Thread.Sleep(500);
            driver.Close();
            Thread.Sleep(500);
        }

        driver.SwitchTo().Window(currentWindowHandle);
        Thread.Sleep(500);

        driver.Navigate().GoToUrl(Program.BaseUrl);
    }


    public static void Click(ChromeDriver driver, IWebElement element)
    {
        try
        {
            element.Click();
        }
        catch
        {
            TryDismissAllMessages(driver);
            element.Click();
        }
    }

    private static void TryDismissAllMessages(ChromeDriver driver)
    {
        var buttons = new List<(By, string)>
        {
            (By.Id("iLandingViewAction"), "iLandingViewAction"),
            (By.Id("iShowSkip"), "iShowSkip"),
            (By.Id("iNext"), "iNext"),
            (By.Id("iLooksGood"), "iLooksGood"),
            (By.Id("idSIButton9"), "idSIButton9"),
            (By.Id("bnp_btn_accept"), "bnp_btn_accept"),
            (By.Id("acceptButton"), "acceptButton")
        };

        foreach (var (by, value) in buttons)
        {
            try
            {
                var elements = driver.FindElements(by);
                foreach (var element in elements)
                {
                    element.Click();
                }
            }
            catch
            {
            }
        }

        TryDismissCookieBanner(driver);
        TryDismissBingCookieBanner(driver);
    }

    private static void TryDismissCookieBanner(ChromeDriver driver)
    {
        try
        {
            var cookieBanner = driver.FindElement(By.Id("cookie-banner"));
            var button = cookieBanner.FindElement(By.TagName("button"));
            button.Click();
        }
        catch
        {
        }
    }

    private static void TryDismissBingCookieBanner(ChromeDriver driver)
    {
        try
        {
            var acceptButton = driver.FindElement(By.Id("bnp_btn_accept"));
            acceptButton.Click();
        }
        catch
        {
        }
    }

    public static (string, string) GetCCodeLang(string lang, string geo)
    {
        if (!string.IsNullOrEmpty(lang) && !string.IsNullOrEmpty(geo)) return (lang, geo);
        try
        {
            var locationInfo = GetIpLocation();
            if (locationInfo != null)
            {
                if (string.IsNullOrEmpty(lang))
                {
                    lang = locationInfo["languages"].Split(',')[0].Split('-')[0];
                }

                if (string.IsNullOrEmpty(geo))
                {
                    geo = locationInfo["country"];
                }
            }
            else
            {
                Console.WriteLine("Rate-limited or location info not available. Returning default.");
                return ("en", "US");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving location. Returning default. Exception: {ex}");
            return ("en", "US");
        }

        return (lang, geo);
    }

    private static Dictionary<string, string>? GetIpLocation()
    {
        using var client = new HttpClient();
        try
        {
            var response = client.GetStringAsync("https://ipapi.co/json/").Result;
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching IP location data: {ex}");
            return null;
        }
    }
    
    public static string? GetEmailCode(string login)
    {
        using var client = new HttpClient();
        try
        {
            // var response = ;
            var jsonMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            var bytes = Encoding.UTF8.GetBytes(jsonMessage?["data"] ?? string.Empty);
            using var stream = new MemoryStream(bytes);
            var message = MimeMessage.Load(stream);
            var regex = MyRegex();
            var match = regex.Match(message.TextBody);
            if (!match.Success) return null;
            var code = match.Groups["code"].Value;
            return code;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Get Email code: {ex}");
            return null;
        }
    }

    [GeneratedRegex(@"(?<code>\d{6})")]
    private static partial Regex MyRegex();
}

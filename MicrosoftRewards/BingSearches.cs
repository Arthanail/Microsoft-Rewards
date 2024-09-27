using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static class BingSearches
{
    private static readonly int[] SourceArray = [30, 90, 102];

    public static RemainingSearches GetRemainingSearches(ChromeDriver driver, bool mobile,
        bool desktopAndMobile = false)
    {
        dynamic dashboard = Utils.GetDashboard(driver);
        var searchPoints = 1;
        var counters = dashboard["userStatus"]["counters"];

        var progressDesktop = (int)counters["pcSearch"][0]["pointProgress"];
        var targetDesktop = (int)counters["pcSearch"][0]["pointProgressMax"];

        if (counters["pcSearch"].Count >= 2)
        {
            progressDesktop += counters["pcSearch"][1]["pointProgress"];
            targetDesktop += counters["pcSearch"][1]["pointProgressMax"];
        }

        if (SourceArray.Contains(targetDesktop))
        {
            searchPoints = 3;
        }
        else if (targetDesktop is 50 or >= 170 or 150)
        {
            searchPoints = 5;
        }

        var remainingDesktop = (targetDesktop - progressDesktop) / searchPoints;
        var remainingMobile = 0;

        if (dashboard["userStatus"]["levelInfo"]["activeLevel"] != "Level1")
        {
            int progressMobile = counters["mobileSearch"][0]["pointProgress"];
            int targetMobile = counters["mobileSearch"][0]["pointProgressMax"];
            remainingMobile = ((targetMobile - progressMobile) / searchPoints);
        }

        if (desktopAndMobile)
        {
            return new RemainingSearches(remainingDesktop, remainingMobile);
        }

        return mobile
            ? new RemainingSearches(0, remainingMobile)
            : new RemainingSearches(remainingDesktop, 0);
    }

    public static List<string> GetGoogleTrends(int wordsCount)
    {
        /* var i = 0;
        var session = new HttpClient();

        while (searchTerms.Count < wordsCount)
        {
            i++;

            var url =
                $"https://trends.google.com/trends/api/dailytrends?hl=en&ed={DateTime.Today.AddDays(-i).ToString("yyyyMMdd")}&geo=US&ns=15";

            var response = session.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch Google Trends. Status code: {response.StatusCode}");
            }

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(responseBody.Substring(6));

            var trendingSearches = json["default"]?["trendingSearchesDays"]?[0]?["trendingSearches"];
            if (trendingSearches != null)
                foreach (var topic in trendingSearches)
                {
                    searchTerms.Add(topic["title"]?["query"]?.ToString().ToLower());
                    searchTerms.AddRange(
                        topic["relatedQueries"].Select(q => q["query"]?.ToString().ToLower())
                    );
                }

            searchTerms = searchTerms.Distinct().ToList();
        }

        if (searchTerms.Count > wordsCount)
        {
            searchTerms = searchTerms.Take(wordsCount).ToList();
        } */
        var searchTerms = new List<string>
        {
            "Best hiking trails near me",
            "How to start a vegetable garden",
            "Top 10 must-visit beaches worldwide",
            "Healthy breakfast recipes",
            "DIY home decor ideas",
            "Latest fashion trends 2024",
            "Beginner yoga poses",
            "Financial planning tips for young adults",
            "Top 5 budget travel destinations",
            "How to improve indoor air quality",
            "Best books to read this year",
            "Home workout routines without equipment",
            "Quick and easy dinner recipes",
            "Tips for improving sleep quality",
            "Popular podcast recommendations",
            "Sustainable living ideas",
            "How to start a side hustle",
            "Meditation techniques for beginners",
            "Top 3 productivity apps",
            "DIY skincare recipes",
            "Fun activities for a rainy day",
            "Healthy snack ideas for work",
            "How to organize a small closet",
            "Local volunteering opportunities",
            "Tips for reducing plastic waste",
            "Beginner painting tutorials",
            "Virtual fitness classes",
            "How to grow your own herbs indoors",
            "Popular board games for adults",
            "Tips for better time management",
            "Online language learning resources",
            "Creative date night ideas",
            "Home office setup inspiration",
            "Stress-relief techniques",
            "How to bake sourdough bread",
            "Beginner photography tips",
            "How to make homemade candles",
            "Easy ways to declutter your home",
            "Outdoor workout ideas",
            "Healthy smoothie recipes",
            "Virtual museum tours",
            "Tips for starting a meditation practice",
            "How to create a capsule wardrobe",
            "Online cooking classes",
            "Budget-friendly home renovation ideas",
            "Tips for growing a balcony garden",
            "Beginner coding tutorials",
            "How to brew the perfect cup of coffee",
            "Creative writing prompts",
            "Indoor activities for kids",
            "How to make a vision board",
            "Self-care ideas for busy professionals",
            "Virtual book club recommendations",
            "How to build a raised garden bed",
            "Beginner sewing projects",
            "Tips for better posture",
            "Online dance classes",
            "Healthy lunch ideas for work",
            "How to start journaling",
            "DIY natural cleaning products",
            "Tips for reducing food waste",
            "Home workout equipment essentials",
            "Virtual cooking classes",
            "How to create a budget plan",
            "Easy ways to save money",
            "Tips for growing your own vegetables",
            "Online art classes",
            "How to make homemade pizza",
            "Sustainable fashion brands",
            "Beginner calligraphy tutorials",
            "Tips for staying motivated",
            "How to style a small living room",
            "Virtual wine tasting experiences",
            "Healthy dinner ideas for the family",
            "How to start a compost bin",
            "Beginner knitting projects",
            "Tips for better sleep hygiene",
            "Online coding bootcamps",
            "How to make a DIY terrarium",
            "Indoor herb garden ideas",
            "Virtual travel experiences",
            "How to create a morning routine",
            "Healthy dessert recipes",
            "Tips for setting achievable goals",
            "How to build a minimalist wardrobe",
            "Online woodworking classes",
            "How to make homemade ice cream",
            "Tips for reducing screen time",
            "Virtual fitness challenges",
            "How to start a gratitude journal",
            "Beginner pottery classes",
            "Tips for practicing mindfulness",
            "How to make homemade bread",
            "Sustainable living blogs to follow",
            "Easy plant-based recipes",
            "How to start a backyard composting",
            "Beginner DIY furniture projects",
            "Tips for creating a relaxing home environment",
            "Online gardening workshops",
            "How to make your own natural skincare products"
        };

        return GetRandomStrings(searchTerms, wordsCount);
    }

    private static List<string> GetRandomStrings(List<string> sourceList, int count)
    {
        var random = new Random();
        var randomStrings = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var randomIndex = random.Next(0, sourceList.Count);
            randomStrings.Add(sourceList[randomIndex]);
        }

        return randomStrings;
    }

    public static void BingSearch(ChromeDriver driver, List<string> trends)
    {
        foreach (var trend in trends)
        {
            var searchbar = driver.FindElement(By.Id("sb_form_q"));
            Utils.SmallSleep();
            searchbar.Clear();
            Utils.TypeText(searchbar, trend);
            Utils.SmallSleep();
            searchbar.Submit();
        }
    }
}

public class RemainingSearches(int desktop, int mobile)
{
    public int Desktop { get; } = desktop;
    public int Mobile { get; } = mobile;

    public int GetTotal()
    {
        return Desktop + Mobile;
    }
}
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static class PunchCards
{
    public static void CompletePromotionalItems(ChromeDriver driver)
    {
        dynamic dashboard = Utils.GetDashboard(driver);
        var promotionalItems = dashboard["promotionalItem"];

        var destUrl = new Uri(promotionalItems["destinationUrl"]);
        var baseUrl = new Uri(Program.BaseUrl);

        var promotionalItemsComplete = (bool)promotionalItems["complete"];

        var values = new[] { 100, 200, 500 };

        if (!values.Contains((int)promotionalItems["pointProgressMax"]) ||
            promotionalItemsComplete ||
            ((destUrl.Host != baseUrl.Host || destUrl.AbsolutePath != baseUrl.AbsolutePath) &&
             destUrl.Host != "www.bing.com")) return;
        var promoItem = driver.FindElement(By.XPath("//*[@id='promo-item']/section/div/div/div/span"));
        Utils.Click(driver, promoItem);
        Utils.VisitNewTab(driver, 8);
    }

    public static void CompletePunchCard(ChromeDriver driver, string url, dynamic childPromotions)
    {
        driver.Navigate().GoToUrl(url);
        foreach (var child in childPromotions)
        {
            if (child["complete"] is not false) continue;
            if (child["promotionType"] == "urlreward")
            {
                var offerCta = driver.FindElement(By.ClassName("offer-cta"));
                Utils.Click(driver, offerCta);
                Utils.VisitNewTab(driver, new Random().Next(13, 17));
            }

            if (child["promotionType"] != "quiz") continue;
            {
                var offerCta = driver.FindElement(By.ClassName("offer-cta"));
                Utils.Click(driver, offerCta);
                Utils.VisitNewTab(driver, 8);
                var counter = driver.FindElement(By.XPath("//*[@id='QuestionPane0']/div[2]"))
                    .GetAttribute("innerHTML");

                var numbers = counter.Substring(1, counter.Length - 2)
                    .Split()
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse);

                var numberOfQuestions = numbers.Max();

                for (var question = 0; question < numberOfQuestions; question++)
                {
                    var questionPane = driver.FindElement(By.XPath(
                        $"//*[@id='QuestionPane{question}']/div[1]/div[2]/a[{new Random().Next(1, 3)}]/div"));
                    Utils.Click(driver, questionPane);
                    Utils.SmallSleep();
                    var answerPane = driver.FindElement(
                        By.XPath($"//*[@id='AnswerPane{question}']/div[1]/div[2]/div[4]/a/div/span/input"));
                    Utils.Click(driver, answerPane);
                    Utils.SmallSleep();
                }

                Utils.LargeSleep();
                Utils.CloseCurrentTab(driver);
            }
        }
    }
}
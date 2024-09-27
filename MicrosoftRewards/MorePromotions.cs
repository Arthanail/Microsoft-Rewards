using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static class MorePromotions
{
    public static void OpenMorePromotionsActivity(ChromeDriver driver, int cardId)
    {
        var element =
            driver.FindElement(
                By.CssSelector($"#more-activities > .m-card-group > .ng-scope:nth-child({cardId + 1}) .ds-card-sec"));
        Utils.Click(driver, element);
        Utils.SwitchToNewTab(driver, 5);
    }

    public static void PerformSearch(ChromeDriver driver, string query)
    {
        var searchbar = driver.FindElement(By.Id("sb_form_q"));
        searchbar.SendKeys(query);
        searchbar.Submit();
    }

    public static void CompleteQuizBasedOnPoints(ChromeDriver driver, int pointProgressMax)
    {
        switch (pointProgressMax)
        {
            case 10:
                DailyTasks.CompleteAbc(driver);
                break;
            case 30:
            case 40:
                DailyTasks.CompleteQuiz(driver);
                break;
            case 50:
                DailyTasks.CompleteThisOrThat(driver);
                break;
        }
    }
}
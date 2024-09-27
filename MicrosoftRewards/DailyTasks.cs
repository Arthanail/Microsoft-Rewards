using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MicrosoftRewards;

public static class DailyTasks
{
    public static void OpenDailySetActivity(ChromeDriver driver, int cardId)
    {
        var dailySets = driver.FindElement(By.XPath(
            $"//*[@id='daily-sets']/mee-card-group[1]/div/mee-card[{cardId}]/div/card-content/mee-rewards-daily-set-item-content/div/a"));
        Utils.Click(driver, dailySets);
        Utils.SwitchToNewTab(driver, 8);
    }

    public static void CompleteThisOrThat(ChromeDriver driver)
    {
        var startQuiz = driver.FindElement(By.XPath("//*[@id='rqStartQuiz']"));
        Utils.Click(driver, startQuiz);
        for (var i = 0; i < 10; i++)
        {
            dynamic correctAnswerCode = driver.ExecuteScript("return _w.rewardsQuizRenderInfo.correctAnswer");
            var (answer1, answer1Code) = GetAnswerAndCode(driver, "rqAnswerOption0");
            var (answer2, answer2Code) = GetAnswerAndCode(driver, "rqAnswerOption1");
            if (answer1Code == correctAnswerCode)
            {
                Utils.Click(driver, answer1);
                Utils.LargeSleep();
            }
            else if (answer2Code == correctAnswerCode)
            {
                Utils.Click(driver, answer2);
                Utils.LargeSleep();
            }
        }

        Utils.CloseCurrentTab(driver);
    }

    public static void CompleteQuiz(ChromeDriver driver)
    {
        var btnAccept = driver.FindElement(By.Id("bnp_btn_accept"));
        Utils.Click(driver, btnAccept);
        var startQuiz = driver.FindElement(By.XPath("//*[@id='rqStartQuiz']"));
        Utils.Click(driver, startQuiz);
        Utils.SmallSleep();
        dynamic currentQuestionNumber = driver.ExecuteScript("return _w.rewardsQuizRenderInfo.currentQuestionNumber");
        dynamic maxQuestions = driver.ExecuteScript("return _w.rewardsQuizRenderInfo.maxQuestions");
        dynamic numberOfOptions = driver.ExecuteScript("return _w.rewardsQuizRenderInfo.numberOfOptions");
        for (; currentQuestionNumber < maxQuestions + 1; currentQuestionNumber++)
        {
            if (numberOfOptions == 8)
            {
                var answers = new List<string>();
                for (var options = 0; options < numberOfOptions; options++)
                {
                    var isCorrectOption = driver.FindElement(By.Id($"rqAnswerOption{options}"))
                        .GetAttribute("iscorrectoption");
                    if (!string.IsNullOrEmpty(isCorrectOption) &&
                        string.Equals(isCorrectOption, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        answers.Add($"rqAnswerOption{options}");
                    }
                }

                foreach (var answerButton in answers.Select(answer => driver.FindElement(By.Id(answer))))
                {
                    Utils.Click(driver, answerButton);
                }
            }
            else if (numberOfOptions == 2 || numberOfOptions == 3 || numberOfOptions == 4)
            {
                var correctOption = driver.ExecuteScript("return _w.rewardsQuizRenderInfo.correctAnswer");
                for (var options = 0; options < numberOfOptions; options++)
                {
                    if (driver.FindElement(By.Id($"rqAnswerOption{options}")).GetAttribute("data-option") !=
                        correctOption.ToString()) continue;
                    var rqAnswerOption = driver.FindElement(By.Id($"rqAnswerOption{options}"));
                    Utils.Click(driver, rqAnswerOption);
                }
            }
        }

        Utils.SmallSleep();
        Utils.CloseCurrentTab(driver);
    }

    public static void CompleteAbc(ChromeDriver driver)
    {
        var counterElement = driver.FindElement(By.XPath("//*[@id='QuestionPane0']/div[2]"));

        var counterText = counterElement.Text;
        if (counterText.Length > 2)
        {
            counterText = counterText.Substring(1, counterText.Length - 2);
        }

        var numberOfQuestions = counterText
            .Split()
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .Max();
        for (var question = 0; question < numberOfQuestions; question++)
        {
            var questionOptionChoice =
                driver.FindElement(By.Id($"questionOptionChoice{question}{new Random().Next(0, 2)}"));
            Utils.Click(driver, questionOptionChoice);
            Utils.LargeSleep();
            var nextQuestionBtn = driver.FindElement(By.Id($"nextQuestionbtn{question}"));
            Utils.Click(driver, nextQuestionBtn);
            Utils.LargeSleep();
            Utils.CloseCurrentTab(driver);
        }
    }

    private static (IWebElement, string?) GetAnswerAndCode(ChromeDriver driver, string answerId)
    {
        var answerEncodeKey = driver.ExecuteScript("return _G.IG").ToString();
        var answer = driver.FindElement(By.Id(answerId));
        var answerTitle = answer.GetAttribute("data-option");

        return answerTitle != null ? (answer, GetAnswerCode(answerEncodeKey, answerTitle)) : (answer, null);
    }

    private static string GetAnswerCode(string? key, string inputString)
    {
        var t = inputString.Aggregate(0, (current, t1) => current + t1);

        t += Convert.ToInt32(key?[^2..], 16);

        return t.ToString();
    }
}
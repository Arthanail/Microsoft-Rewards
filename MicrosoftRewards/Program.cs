using Colorify;
using Colorify.UI;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MicrosoftRewards;

internal class Program
{
    private static Format Colorify { get; } = new(Theme.Dark);

    public const string BaseUrl = "https://rewards.bing.com";
    public const string SearchUrl = "https://bing.com/";

    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/logfile.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
            })
            .Build();

        var loginArg = args[1];
        var passwordArg = args[3];

        var options = new ChromeOptions();
        if (args.Length > 5 && args[5] != "")
        {
            options.AddExtension(Path.Combine(AppContext.BaseDirectory, $"proxy/{args[5]}"));
        }
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--blink-settings=imagesEnabled=false");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--ignore-certificate-errors-spki-list");
        options.AddArgument("--ignore-ssl-errors");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--dns-prefetch-disable");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-features=Translate");
        options.AddArgument("--disable-features=PrivacySandboxSettings4");
        options.AddArgument("--disable-search-engine-choice-screen");

        var driver = new ChromeDriver(options);

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
        driver.Manage().Window.Size = new System.Drawing.Size(new Random().Next(1024, 2056), new Random().Next(768, 1700));

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
            
        try
        {
            // Login
            Colorify.WriteLine($"[LOGIN] Login Account: {loginArg}", Colors.txtInfo);
            logger.LogInformation("[LOGIN] Login Account: {@LoginArg}", loginArg);
            try
            {
                Actions.Login(driver, loginArg, passwordArg);
            }
            catch (Exception e)
            {
                Colorify.WriteLine($"[LOGIN] Login Failed: {loginArg}", Colors.txtWarning);
                logger.LogInformation("[LOGIN] Login Failed: {@LoginArg} {@Exception}", loginArg, e.Message);
            }

            Colorify.WriteLine($"[LOGIN] Login Completed: {loginArg}", Colors.txtSuccess);
            logger.LogInformation("[LOGIN] Login Completed: {@LoginArg}", loginArg);

            dynamic dashboard = Utils.GetDashboard(driver);
            long startingPoints = dashboard["userStatus"]["availablePoints"];
            Colorify.WriteLine($"Available Points: {startingPoints}", Colors.txtInfo);

            // Daily Tasks
            Colorify.WriteLine($"[DAILY TASKS] Trying Complete Daily Tasks: {loginArg}", Colors.txtInfo);
            logger.LogInformation("[DAILY TASKS] Trying Complete Daily Tasks: {@LoginArg}", loginArg);
            try
            {
                Actions.CompleteDailyTasks(driver);
            }
            catch (Exception e)
            {
                logger.LogInformation("[DAILY TASKS] Daily Tasks Failed: {@LoginArg} {@Exception}", loginArg,
                    e.Message);
            }

            Colorify.WriteLine($"[DAILY TASKS] Daily Tasks Completed: {loginArg}", Colors.txtSuccess);
            logger.LogInformation("[DAILY TASKS] Daily Tasks Completed: {@LoginArg}", loginArg);

            // Punch Cards
            Colorify.WriteLine($"[PUNCH CARDS] Trying Complete Punch Cards: {loginArg}", Colors.txtInfo);
            logger.LogInformation("[PUNCH CARDS] Trying Complete Punch Cards: {@LoginArg}", loginArg);
            try
            {
                Actions.CompletePunchCards(driver);
            }
            catch (Exception e)
            {
                logger.LogInformation("[PUNCH CARDS] Error While Complete Punch Cards: {@LoginArg} {@Exception}",
                    loginArg, e.Message);
            }

            Colorify.WriteLine($"[PUNCH CARDS] Punch Cards Completed: {loginArg}", Colors.txtSuccess);
            logger.LogInformation("[PUNCH CARDS] Punch Cards Completed: {@LoginArg}", loginArg);

            // More Promotions
            Colorify.WriteLine($"[MORE PROMOTIONS] Trying Complete More Promotions: {loginArg}", Colors.txtInfo);
            logger.LogInformation("[MORE PROMOTIONS] Trying Complete More Promotions: {@LoginArg}", loginArg);
            try
            {
                Actions.CompleteMorePromotions(driver, Colorify);
            }
            catch (Exception e)
            {
                logger.LogInformation("[MORE PROMOTIONS] More Promotions Failed: {@LoginArg} {@Exception}", loginArg,
                    e.Message);
            }

            Colorify.WriteLine("[MORE PROMOTIONS] More Promotions Completed", Colors.txtSuccess);
            logger.LogInformation("[MORE PROMOTIONS] More Promotions Completed: {@LoginArg}", loginArg);

            // Bing Search
            Colorify.WriteLine($"[BING] Trying Complete Browser Bing Searches: {loginArg}", Colors.txtInfo);
            logger.LogInformation("[BING] Trying Complete Browser Bing Searches: {@LoginArg}", loginArg);
            try
            {
                Actions.CompleteBrowserBingSearches(driver, logger, Colorify);
            }
            catch (Exception e)
            {
                logger.LogInformation("[BING] Bing Searches Failed: {@LoginArg} {@Exception}", loginArg, e.Message);
            }

            Colorify.WriteLine($"[BING] Browser Bing Searches Completed: {loginArg}", Colors.txtSuccess);
            logger.LogInformation("[BING] Browser Bing Searches Completed: {@LoginArg}", loginArg);

            driver.Navigate().GoToUrl(BaseUrl);
            Utils.LargeSleep();
            dashboard = Utils.GetDashboard(driver);
            long accountPoints = dashboard["userStatus"]["availablePoints"];

            Colorify.WriteLine($"Account: {loginArg}", Colors.txtInfo);
            Colorify.WriteLine($"Points earned today: {accountPoints - startingPoints}", Colors.txtInfo);
            Colorify.WriteLine($"Total points: {accountPoints}", Colors.txtInfo);

            // Actions.BingLogin(driver);
        }
        finally
        {
            Log.CloseAndFlush();
            driver.Close();
            driver.Quit();
        }
    }
}
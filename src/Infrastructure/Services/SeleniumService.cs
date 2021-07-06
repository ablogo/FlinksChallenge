using Core.Constants;
using Core.Dtos;
using Core.Interfaces;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SeleniumService : ISelenium
    {
        private readonly FlinksConfiguration _flinksConfig;
        private readonly ChromeDriverConfiguration _chromeDriverConfig;
        private readonly ILog _log;

        public SeleniumService(ILog log, IOptions<FlinksConfiguration> flinksConfig, IOptions<ChromeDriverConfiguration> chromeDriverConfig)
        {
            _flinksConfig = flinksConfig.Value;
            _chromeDriverConfig = chromeDriverConfig.Value;
            _log = log;
        }

        public async Task<LoginResponse> Login(string user, string password, string challengeId, Dictionary<string, string> cookies, bool twiceLogin, ProxyInfo proxyInfo)
        {
            LoginResponse response = new LoginResponse();
            ChromeDriver driver = null;

            try
            {
                var random = new Random();
                int index = random.Next(UserAgents.UserAgentStrings.Length);
                string userAgent = UserAgents.UserAgentStrings[index];
                string url = string.IsNullOrEmpty(challengeId) ? _flinksConfig.Url : _flinksConfig.Url + "/Authorize/" + challengeId;

                ChromeOptions options = new ChromeOptions();
                options.BinaryLocation = _chromeDriverConfig.ChromePath;
                options.AddArgument($"user-agent={userAgent}");
                options.AddArgument("ignore-certificate-errors");
                options.AddExcludedArgument("enable-automation");
                options.AddArgument("incognito");

                if (proxyInfo != null)
                {
                    Proxy proxy = new Proxy();
                    proxy.Kind = ProxyKind.Manual;
                    proxy.IsAutoDetect = false;
                    proxy.SslProxy = proxyInfo.Ip + ":" + proxyInfo.Port;
                    options.Proxy = proxy;
                }


                // Create an instance of the browser with configure launch options
                driver = new ChromeDriver(options);

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

                driver.Navigate().GoToUrl(url);

                if (string.IsNullOrEmpty(challengeId))
                {
                    challengeId = driver.FindElement(By.CssSelector("body > div > div > div:nth-child(2) > p:nth-child(11) > b")).Text;
                    var startButton = driver.FindElement(By.CssSelector("body > div > div > div:nth-child(2) > p:nth-child(12) > a"));
                    startButton.Click();
                }
                else
                {
                    driver.Manage().Cookies.DeleteAllCookies();
                    foreach (var item in cookies)
                    {
                        driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(item.Key, item.Value));
                    }
                }

                response = AttemptLogin(ref driver, user, password, twiceLogin);
                response.ChallengeId = challengeId;


            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            finally
            {
                driver?.Close();
                driver?.Dispose();
            }

            return response;
        }

        private LoginResponse AttemptLogin(ref ChromeDriver driver, string user, string password, bool twiceLogin)
        {
            LoginResponse response = new LoginResponse();
            try
            {
                driver.ExecuteChromeCommand("Runtime.evaluate", new Dictionary<string, object>() { { "expression", "numberOfOccurenceMove = 9;" } });

                var loginButton = driver.FindElement(By.CssSelector("button[type=\"submit\"]"));

                Actions actionProvider = new Actions(driver);
                // Performs mouse move action.
                actionProvider.MoveToElement(loginButton).Build().Perform();

                var userInput = driver.FindElement(By.CssSelector("input[name^=\"username\"]"));
                userInput.SendKeys(user);
                var passwordInput = driver.FindElement(By.CssSelector("input[name^=\"password\"]"));
                passwordInput.SendKeys(password);

                loginButton.Click();

                response = GetToken(ref driver, user, password, twiceLogin);

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return response;
        }

        private LoginResponse GetToken(ref ChromeDriver driver, string user, string password, bool twiceLogin)
        {
            LoginResponse response = new LoginResponse();
            try
            {
                // Search message login
                var cookies = new Dictionary<string, string>();
                var congratsDiv = driver.FindElement(By.XPath("/html/body/div/div/div[2]"));
                var loginMessage = congratsDiv.FindElement(By.CssSelector(":first-child")).Text;

                if (loginMessage.StartsWith("Congrats"))
                {
                    foreach (var item in driver.Manage().Cookies.AllCookies)
                    {
                        if (cookies.ContainsKey(item.Name))
                        {
                            cookies[item.Name] = item.Value;
                        }
                        else
                        {
                            cookies.Add(item.Name, item.Value);
                        }
                    }
                    response.Cookies = cookies;

                    var authToken = driver.FindElement(By.CssSelector("body > div > div > div:nth-child(2) > p:nth-child(2) > b"));
                    response.Token = authToken.Text;
                    response.Result = true;
                }
                else if (twiceLogin)
                {
                    // Set to false to avoid infinite loop
                    response = AttemptLogin(ref driver, user, password, false);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return response;
        }


    }
}

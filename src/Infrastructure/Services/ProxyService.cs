using Core.Dtos;
using Core.Interfaces;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProxyService : IProxy
    {
        private readonly ChromeDriverConfiguration _chromeDriverConfig;
        private readonly ILog _log;

        public ProxyService(ILog log, IOptions<ChromeDriverConfiguration> chromeDriverConfig)
        {
            _chromeDriverConfig = chromeDriverConfig.Value;
            _log = log;
        }

        public async Task<List<ProxyInfo>> GetProxies()
        {
            List<ProxyInfo> proxies = new List<ProxyInfo>();

            try
            {
                ChromeOptions options = new ChromeOptions();
                options.BinaryLocation = _chromeDriverConfig.ChromePath;
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--incognito");
                options.AddExcludedArgument("enable-automation");


                // Create an instance of the browser with configure launch options
                // Use this page to obtain free https servers
                using (IWebDriver driver = new ChromeDriver(options))
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

                    driver.Navigate().GoToUrl("http://www.freeproxylists.net/?c=&pt=&pr=HTTPS&a%5B%5D=0&a%5B%5D=1&a%5B%5D=2&u=80");

                    var proxyTable = driver.FindElements(By.CssSelector("body > div:nth-child(3) > div:nth-child(2) > table > tbody > tr"));

                    foreach (var item in proxyTable.Skip(1))
                    {
                        if (!string.IsNullOrEmpty(item.Text))
                        {
                            proxies.Add(new ProxyInfo()
                            {
                                Ip = item.FindElement(By.CssSelector("td > a")).Text,
                                Port = item.FindElement(By.CssSelector("td:nth-child(2)")).Text
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }

            return proxies;
        }
    }
}

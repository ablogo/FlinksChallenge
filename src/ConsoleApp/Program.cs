using Core.Dtos;
using Core.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, start challenge!");
                GetTokens().Wait();
            }
            catch (Exception ex)
            {
            }
        }

        private static async Task GetTokens()
        {
            try
            {
                var services = ConfigureServices();
                var serviceProvider = services.AddLogging(x => x.ClearProviders().AddConsole()).BuildServiceProvider();

                var loginResponse = new List<LoginResponse>();
                var currentLogin = new LoginResponse();
                var proxyInfo = new List<ProxyInfo>();
                string challengeId = "";
                Dictionary<string, string> cookies = null;
                int count = 0;

                do
                {

                    currentLogin = await serviceProvider.GetService<ISelenium>().Login("2222", "2222", challengeId, cookies, (count > 20 ? true : false), (proxyInfo.Count == 0 ? null : proxyInfo[0]));
                    if (currentLogin.Result)
                    {
                        loginResponse.Add(currentLogin);
                        count++;
                    }
                    else
                    {
                        if (proxyInfo.Count == 0)
                        {
                            proxyInfo = await serviceProvider.GetService<IProxy>().GetProxies();
                        }
                        else
                        {
                            proxyInfo.RemoveAt(0);
                        }
                    }

                    if (string.IsNullOrEmpty(challengeId))
                    {
                        challengeId = currentLogin.ChallengeId;
                        cookies = currentLogin.Cookies;
                    }

                }
                while (loginResponse.Count < 50);

                Console.WriteLine("Challenge done maybe." + Environment.NewLine + "ChallengeId: " + challengeId);
                loginResponse.ForEach(x => Console.WriteLine(x.Token));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            try
            {
                var configuration = LoadConfiguration();
                services.AddSingleton(configuration);

                services.AddSingleton<ILog, LogService<SeleniumService>>();
                services.AddTransient<ISelenium, SeleniumService>();
                services.AddSingleton<IProxy, ProxyService>();

                services.Configure<FlinksConfiguration>(configuration.GetSection("Flinks"));
                services.Configure<ChromeDriverConfiguration>(configuration.GetSection("ChromeDriver"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}

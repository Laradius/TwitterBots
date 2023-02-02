using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterBot
{
    internal class MyApp
    {
        private readonly ILogger<MyApp> _logger;
        private readonly AppSettings _appSettings;
        private readonly IBot _horrorBot;
        private readonly IBot _psychologyBot;

        public MyApp(IOptions<AppSettings> appSettings, ILogger<MyApp> logger, IServiceProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _horrorBot = provider.GetServices<IBot>().First(o => o.GetType() == typeof(HorrorBot));
            _psychologyBot = provider.GetServices<IBot>().First(o => o.GetType() == typeof(PsychologyBot));
        }

        public async Task Run(string[] args)

        {
            await _psychologyBot.GenerateTweet();
            await _horrorBot.GenerateTweet();
            _logger.LogInformation("Writing tweets on bot startup...");

            await PeriodicTask.Run(() =>
            {
                var psychologyTweet = _psychologyBot.GenerateTweet();
                var horrorTweet = _horrorBot.GenerateTweet();
                _logger.LogInformation("Writing tweets per hour...");
            }, TimeSpan.FromHours(1));

            await Task.CompletedTask;
        }
    }
}
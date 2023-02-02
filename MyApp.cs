using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
            await Task.WhenAll(_psychologyBot.GenerateTweet(), _horrorBot.GenerateTweet());
            _logger.LogInformation("Writing tweets on bot startup...");

            var repeatInterval = 60;
            var timerInterval = 10;

            var remainingTime = TimeSpan.FromMinutes(repeatInterval);

            var timer = new System.Timers.Timer(TimeSpan.FromMinutes(timerInterval).TotalMilliseconds);

            timer.Elapsed += (object source, ElapsedEventArgs e) =>
            {
                remainingTime -= TimeSpan.FromMinutes(timerInterval);
                _logger.LogInformation($"Minutes remaining till next post: {remainingTime.TotalMinutes}");

                if (remainingTime <= TimeSpan.Zero)
                {
                    timer.Enabled = false;
                }
            };
            timer.AutoReset = true;
            timer.Enabled = true;

            await PeriodicTask.Run(async () =>
            {
                await Task.WhenAll(_psychologyBot.GenerateTweet(), _horrorBot.GenerateTweet());
                _logger.LogInformation("Writing posts from PeriodicTask...");
                timer.Enabled = true;
                remainingTime = TimeSpan.FromMinutes(repeatInterval);
            }, TimeSpan.FromMinutes(repeatInterval));

            await Task.CompletedTask;
        }
    }
}
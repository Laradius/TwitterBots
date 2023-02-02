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

        public MyApp(IOptions<AppSettings> appSettings, ILogger<MyApp> logger, IServiceProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _horrorBot = provider.GetServices<IBot>().First(o => o.GetType() == typeof(HorrorBot));
        }

        public async Task Run(string[] args)

        {
            await _horrorBot.GenerateTweet();
            await Task.CompletedTask;
        }
    }
}
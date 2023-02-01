using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;

namespace TwitterBot
{
    internal sealed class HorrorBot : IBot
    {
        public TwitterClient Client { get; set; }

        public HorrorBot(IOptions<AppSettings> _settings)
        {
            var settings = _settings.Value;

            Client = new TwitterClient(settings.ApiKey, settings.ApiSecret, settings.HorrorToken.Public, settings.HorrorToken.Secret);
        }
    }
}
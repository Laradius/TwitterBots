using Autofac.Core;
using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using TwitterBot.Models;

namespace TwitterBot
{
    internal interface IBot
    {
        public delegate IBot BotResolver(string key);

        public Task GenerateTweet();

        protected Task<Result<string>> GenerateImage(string desc);

        protected Task<TweetContent> GenerateContent();

        protected TwitterClient Client { get; set; }
    }
}
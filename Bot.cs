using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;

namespace TwitterBot
{
    internal interface IBot
    {
        public delegate IBot BotResolver(string key);

        public TwitterClient Client { get; protected set; }
    }
}
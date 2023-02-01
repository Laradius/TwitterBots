using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TwitterBot.Models;

namespace TwitterBot
{
    internal class AppSettings
    {
        public string OpenAIKey { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public AccessToken PsychologyToken { get; set; }
        public AccessToken HorrorToken { get; set; }
    }
}
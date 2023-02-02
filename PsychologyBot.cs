using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI.Completions;
using OpenAI.Models;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Parameters;
using Tweetinvi;
using TwitterBot.Models;

namespace TwitterBot
{
    internal sealed class PsychologyBot : IBot
    {
        public TwitterClient Client { get; set; }

        private readonly string[] _psychologyIdeas;
        private readonly string[] _psychologyInspirational;
        private readonly string[] _mentalHealthImprovement;
        private readonly string[] _psychologyImages;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PsychologyBot> _logger;

        public PsychologyBot(IOptions<AppSettings> _settings, ILogger<PsychologyBot> logger)
        {
            _appSettings = _settings.Value;
            _logger = logger;

            var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("JsonDatabase.json"));

            _mentalHealthImprovement = data["mentalHealthImprovement"].ToObject<string[]>();
            _psychologyIdeas = data["psychologyIdeas"].ToObject<string[]>();
            _psychologyImages = data["psychologyImages"].ToObject<string[]>();
            _psychologyInspirational = data["psychologyInspirational"].ToObject<string[]>();

            Client = new TwitterClient(_appSettings.ApiKey, _appSettings.ApiSecret, _appSettings.PsychologyToken.Public, _appSettings.PsychologyToken.Secret);
        }

        public async Task GenerateTweet()
        {
            var content = await GenerateContent();
            var uploadedImage = await Client.Upload.UploadTweetImageAsync(content.Image);

            var tweetWithImage = await Client.Tweets.PublishTweetAsync(new PublishTweetParameters(content.Text)
            {
                Medias = { uploadedImage }
            });

            _logger.LogInformation("Published psychology tweet.");
        }

        public async Task<Result<string>> GenerateImage(string desc)
        {
            OpenAIClient api = new OpenAIClient(new OpenAIAuthentication(_appSettings.OpenAIKey), Model.Davinci);
            IReadOnlyList<string> data;

            data = await api.ImagesEndPoint.GenerateImageAsync(desc, 1, OpenAI.Images.ImageSize.Medium);
            return data[0];
        }

        public async Task<TweetContent> GenerateContent()

        {
            TweetContent content = new TweetContent();
            Random rand = new Random();
            CompletionResult promptResult;
            OpenAIClient api = new OpenAIClient(new OpenAIAuthentication(_appSettings.OpenAIKey), Model.Davinci);

            switch (rand.Next(0, 3))
            {
                case 0:
                    promptResult = await api.CompletionsEndpoint.CreateCompletionAsync($"Write about mental health impacts of {_mentalHealthImprovement[rand.Next(0, _mentalHealthImprovement.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters", temperature: 0.75, max_tokens: 256);
                    break;

                case 1:
                    promptResult = await api.CompletionsEndpoint.CreateCompletionAsync($"Write about psychological concepts of {_psychologyIdeas[rand.Next(0, _psychologyIdeas.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters", temperature: 0.75, max_tokens: 256);
                    break;

                case 2:
                    promptResult = await api.CompletionsEndpoint.CreateCompletionAsync($"Write inspirational text about {_psychologyInspirational[rand.Next(0, _psychologyInspirational.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters", temperature: 0.75, max_tokens: 256);
                    break;

                default:
                    throw new ArgumentException("Switch argument is too large/small");
            }

            var imageResult = await GenerateImage(_psychologyImages[rand.Next(0, _psychologyImages.Length)]);
            string imgUrl = "";
            imageResult.IfFail((ex) =>
            {
                _logger.LogError("Failed to generate image for psychology bot. Should never happen.");
            });
            imageResult.IfSucc((result) =>
            {
                imgUrl = result.ToString();
            });

            content.Text = promptResult.ToString();

            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(imgUrl))
                {
                    content.Image =
                        await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }

            return content;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI.Completions;
using OpenAI.Models;
using OpenAI;
using Tweetinvi;
using Microsoft.Extensions.Logging;
using TwitterBot.Models;
using LanguageExt.Common;
using LanguageExt;
using System.Reflection.Metadata.Ecma335;
using System;
using LanguageExt.ClassInstances;
using Tweetinvi.Parameters;

namespace TwitterBot
{
    internal sealed class HorrorBot : IBot
    {
        public TwitterClient Client { get; set; }

        private readonly string[] _horrorIdeas;
        private readonly string[] _customPrompts;
        private readonly string[] _fallbackImages;
        private readonly AppSettings _appSettings;
        private readonly ILogger<HorrorBot> _logger;

        public HorrorBot(IOptions<AppSettings> _settings, ILogger<HorrorBot> logger)
        {
            _appSettings = _settings.Value;
            _logger = logger;

            var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("JsonDatabase.json"));

            _horrorIdeas = data["horrorIdeas"].ToObject<string[]>();
            _customPrompts = data["customHorrorPrompts"].ToObject<string[]>();
            _fallbackImages = data["fallbackHorrorImages"].ToObject<string[]>();

            Client = new TwitterClient(_appSettings.ApiKey, _appSettings.ApiSecret, _appSettings.HorrorToken.Public, _appSettings.HorrorToken.Secret);
        }

        public async Task GenerateTweet()
        {
            var content = await GenerateContent();
            var uploadedImage = await Client.Upload.UploadTweetImageAsync(content.Image);

            var tweetWithImage = await Client.Tweets.PublishTweetAsync(new PublishTweetParameters(content.Text)
            {
                Medias = { uploadedImage }
            });
        }

        public async Task<Result<string>> GenerateImage(string desc)
        {
            OpenAIClient api = new OpenAIClient(new OpenAIAuthentication(_appSettings.OpenAIKey), Model.Davinci);
            IReadOnlyList<string> data;
            try
            {
                data = await api.ImagesEndPoint.GenerateImageAsync(desc, 1, OpenAI.Images.ImageSize.Medium);
                return data[0];
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to generate image with this prompt, trying fallback");
            }
            Random rand = new Random();

            data = await api.ImagesEndPoint.GenerateImageAsync(_fallbackImages[rand.Next(0, _fallbackImages.Length)], 1, OpenAI.Images.ImageSize.Medium);

            return data[0];
        }

        public async Task<TweetContent> GenerateContent()

        {
            TweetContent content = new TweetContent();
            Random rand = new Random();

            OpenAIClient api = new OpenAIClient(new OpenAIAuthentication(_appSettings.OpenAIKey), Model.Davinci);

            CompletionResult promptResult;
            if (rand.Next(0, 10) == 0)
            {
                promptResult = await api.CompletionsEndpoint.CreateCompletionAsync($"{_customPrompts[rand.Next(0, _customPrompts.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters", temperature: 0.75, max_tokens: 256);
            }

            promptResult = await api.CompletionsEndpoint.CreateCompletionAsync($"Write short horror story about {_horrorIdeas[rand.Next(0, _horrorIdeas.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters.", temperature: 0.75, max_tokens: 256);
            var imageResult = await GenerateImage(promptResult.ToString());
            string imgUrl = "";
            imageResult.IfFail((ex) =>
            {
                _logger.LogWarning("Failed to generate image even with fallback.");
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
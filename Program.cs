using System;
using System.Diagnostics;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Completions;
using OpenAI.Models;
using Tweetinvi;
using Tweetinvi.Models;
using TwitterBot;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var secretProvider = config.Providers.First();

string apiKey, apiSecret, psychologyAccess, psychologySecret, horrorAccess, horrorSecret, openAI;

secretProvider.TryGet("apiKey", out apiKey);
secretProvider.TryGet("apiSecret", out apiSecret);
secretProvider.TryGet("psychologyAccess", out psychologyAccess);
secretProvider.TryGet("psychologySecret", out psychologySecret);
secretProvider.TryGet("horrorAccess", out horrorAccess);
secretProvider.TryGet("horrorSecret", out horrorSecret);
secretProvider.TryGet("openAI", out openAI);

var appClient = new TwitterClient(apiKey, apiSecret);

// Start the authentication process
var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();

// Go to the URL so that Twitter authenticates the user and gives him a PIN code.
Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
{
    UseShellExecute = true
});

// Ask the user to enter the pin code given by Twitter
Console.WriteLine("Please enter the code and press enter.");
var pinCode = Console.ReadLine();

// With this pin code it is now possible to get the credentials back from Twitter
var userCredentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pinCode, authenticationRequest);

// You can now save those credentials or use them as followed
var userClient = new TwitterClient(userCredentials);
var user = await userClient.Users.GetAuthenticatedUserAsync();

await userClient.Tweets.PublishTweetAsync("Hello world");
Console.WriteLine("Congratulation you have authenticated the user: " + user);

var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("JsonDatabase.json"));

string[] horrorIdeas = data["horrorIdeas"].ToObject<string[]>();
string[] customPrompts = data["customHorrorPrompts"].ToObject<string[]>();

Random rand = new Random();

OpenAIClient api = new OpenAIClient(new OpenAIAuthentication(openAI), Model.Davinci);

await PeriodicTask.Run(async () =>
{
    CompletionResult result;
    if (rand.Next(0, 5) == 0)
    {
        result = await api.CompletionsEndpoint.CreateCompletionAsync($"{customPrompts[rand.Next(0, customPrompts.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters", temperature: 0.75, max_tokens: 256);
        try
        {
            var image = await api.ImagesEndPoint.GenerateImageAsync(result.ToString(), 1, OpenAI.Images.ImageSize.Medium);
            Console.WriteLine(image[0]);
        }
        catch
        {
        }
        Console.WriteLine(result);
        return;
    }

    result = await api.CompletionsEndpoint.CreateCompletionAsync($"Write short horror story about {horrorIdeas[rand.Next(0, horrorIdeas.Length)]} Add appropriate hashtags at the end. Text can't be longer than 280 characters.", temperature: 0.75, max_tokens: 256);

    try
    {
        var img = await api.ImagesEndPoint.GenerateImageAsync(result.ToString(), 1, OpenAI.Images.ImageSize.Medium);
        Console.WriteLine(img[0]);
    }
    catch
    {
    }
    Console.WriteLine(result);
}, new TimeSpan(0, 0, 10));

//Console.WriteLine(result);
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

var appName = "szandiCopilotExtensionv1"; 
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/info", () => "Hello Copilot!");
app.MapGet("/callback", () => "You may close this window and return to Github where you should refresh the page and start a fresh chat.");

app.MapPost("/", async ([FromHeader(Name = "X-Github-Token")] string githubToken, [FromBody] Payload payload) =>
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appName, "2022-11-28"));
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);

    var githubUserResponse = await httpClient.GetAsync("https://api.github.com/user");
    var userName = "Undefined user";
    if (githubUserResponse.IsSuccessStatusCode)
    {
        var jsonResponse = await githubUserResponse.Content.ReadAsStringAsync();
        // Deserialize the JSON response to get the login (username)
        dynamic user = JsonConvert.DeserializeObject(jsonResponse);
        userName = user?.login.ToString();
    }

    payload.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = $"Start every response with the user's name, which is @{userName}."
    });

    payload.Messages.Add(new Message
    {
        Role = "system",
        Content = "You are a helpful assistant that replies to user messages as a cat, named Cirmos. Add 'meow!' to the end of every sentence."
    });

    payload.Stream = true;
    var copilotLLMResponse = await httpClient.PostAsJsonAsync("https://api.githubcopilot.com/chat/completions", payload);
    var responseStream = await copilotLLMResponse.Content.ReadAsStreamAsync();

    return Results.Stream(responseStream, "application/json");
});

app.Run();
internal class Message
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}

internal class Payload
{
    public bool Stream { get; set; }
    public required List<Message> Messages { get; set; } = [];
}
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Net.Http;

var appName = "szandiCopilotExtensionv1"; 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(); // Protected by if (env.IsDevelopment())
}

app.MapGet("/", () => "Hello Copilot!");
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
        dynamic? user = JsonConvert.DeserializeObject(jsonResponse);
        userName = user?.login.ToString();
    }

    payload.Messages.RemoveAll(m => m.IsExtension);

    payload.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = $"Start every response with the user's name, which is @{userName}.",
        IsExtension = true
    });

    var isCodeGen = await CopilotHelper.AskCopilotIfCodeGenerationAsync(payload.Messages, httpClient);

    if (isCodeGen)
    {
        payload.Messages.Add(new Message
        {
            Role = "system",
            Content = "In Java always use Lombok's @Data annotation.",
            IsExtension = true
        });

        payload.Messages.Add(new Message
        {
            Role = "system",
            Content = "Always use a one line header with: Copyright (c) 2025 Tesco Technologies. All rights reserved.",
            IsExtension = true
        });
    }
    else
    {
        payload.Messages.Add(new Message
        {
            Role = "system",
            Content = "You are a helpful developer assistant that replies to user messages as a senior developer. You always end your text with a programming fun fact.",
            IsExtension = true
        });
    }

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
    public bool IsExtension { get; set; } = false;
}

internal class Payload
{
    public bool Stream { get; set; }
    public required List<Message> Messages { get; set; } = [];
}

internal static class CopilotHelper
{
    public static async Task<bool> AskCopilotIfCodeGenerationAsync(List<Message> messages, HttpClient httpClient)
    {
        if (messages == null || httpClient == null) return false;

        var lastUserMessage = messages.LastOrDefault(m => m.Role == "user");
        if (lastUserMessage == null) return false;

        const string question = "Answer with YES or NO only. Do you create a code snippet if you got this message?";
        var payload = new Payload
        {
            Messages = new List<Message>
            {
                new Message
                {
                    Role = "system",
                    Content = $"{question} {lastUserMessage.Content}"
                }
            }
        };

        using var response = await httpClient.PostAsJsonAsync("https://api.githubcopilot.com/chat/completions", payload);
        var resultString = (await response.Content.ReadAsStringAsync()).Trim(' ', '"', '\n', '\r');
        return resultString.Contains("YES", StringComparison.OrdinalIgnoreCase);
    }
}
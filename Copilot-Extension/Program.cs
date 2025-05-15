using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

var appName = "szandiCopilotExtensionv1"; 
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/info", () => "Hello Copilot!");
app.MapGet("/callback", () => "You may close this window and return to Github where you should refresh the page and start a fresh chat.");

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
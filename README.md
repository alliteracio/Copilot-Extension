# szandiCopilotExtensionv1

A lightweight ASP.NET Core web service that acts as a proxy between GitHub Copilot chat and your GitHub account, enabling personalized Copilot chat experiences. The service injects your GitHub username and custom system prompts into Copilot chat requests.

## How to use

1. **Install the GitHub App**

   To use this service, install the GitHub App on your account:
   [szandiCopilotExtensionv1 GitHub App](https://github.com/apps/szandicopilotextensionv1)

3. **Try it**

   Start every Copilot msg with @szandiCopilotExtensionv1
   
   Example: "@szandiCopilotExtensionv1 What are anonimus functions in C#?"
   
# Copilot-Extension

Initial scaffolding for a GitHub Copilot extension. This repository contains the basic structure and setup needed to start developing a custom Copilot extension, including project boilerplate, minimal configuration files, and minimal implementation to build upon.

## Features

- Exposes endpoints for Copilot chat integration.
- Authenticates users via a GitHub token.
- Fetches the GitHub username and customizes chat prompts.
- Adds fun, cat-themed responses to Copilot chat.

## Endpoints

- `GET /info`  
  Returns a simple greeting to verify the service is running.

- `GET /callback`  
  Displays a message for users returning from GitHub.

- `POST /`  
  Accepts a Copilot chat payload and a GitHub token, injects the username, and proxies the request to the Copilot chat API.
  
## Create your own extension

   
  - Clone it.
    
        git clone https://github.com/alliteracio/Copilot-Extension.git
  
  - Modify the `var appName` variable. (It has to be unique.)
  
  - Modify the `POST /` endpoint, keep your GitHub token in the `X-Github-Token` header and modify the Copilot chat payload in the request body.

  - Run. (The service will start on the default ASP.NET Core port (e.g., `http://localhost:5000`).)
    
        cd Copilot-Extension dotnet run
  
  - Create a dev tunnel.
    
        https://learn.microsoft.com/en-us/connectors/custom-connectors/port-tunneling
  
  - Create a Github Apps. (Use the dev tunnels URL for setting the callback and Copilots URL and use the predefined `appName`.)
    
        https://docs.github.com/en/apps/creating-github-app

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A valid GitHub token with appropriate scopes (e.g., `read:user`)

## License

MIT

---

> **Note:** This project is not affiliated with or endorsed by GitHub or GitHub Copilot.


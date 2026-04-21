# AI-3026 Smart Home — Azure AI Foundry Agent Demo

A browser-based smart home simulator where users control room lights through natural language. An Azure AI Foundry agent (gpt-4o) interprets commands and invokes local tool functions to toggle lights on/off in real time.

## Overview

| Field              | Value                                         |
| ------------------ | --------------------------------------------- |
| **Course**         | AI-3026 (Agents)                              |
| **Application**    | .NET 10 Razor Pages                           |
| **Hosting**        | Azure App Service (zip deploy via azd)        |
| **AI Backend**     | Azure AI Foundry Agent Service (gpt-4o)       |
| **Region**         | swedencentral                                 |
| **Authentication** | System-assigned Managed Identity (no secrets) |

## Architecture

| Resource            | SKU / Config      | Purpose                           |
| ------------------- | ----------------- | --------------------------------- |
| AI Services Account | S0                | Foundry agent + gpt-4o deployment |
| AI Hub              | Basic             | Parent workspace for AI projects  |
| AI Project          | Basic             | Foundry project (agent scope)     |
| App Service Plan    | P1v3              | Compute for web app               |
| App Service         | .NET 10           | Razor Pages host                  |
| Log Analytics       | PerGB2018         | Diagnostics and logging           |
| Model Deployment    | Standard, 30K TPM | gpt-4o for agent reasoning        |

## Features

- SVG blueprint-style floor plan with 5 rooms (Kitchen, Living Room, Bedroom, Bathroom, Garage)
- Natural language chat interface for light control
- AI agent with 4 tool functions: `get_all_light_status`, `get_room_light_status`, `turn_light_on`, `turn_light_off`
- Per-user session isolation (in-memory state, no database required)
- Real-time visual updates when lights toggle

## Prerequisites

- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [.NET 10 SDK](https://dot.net/download)
- Azure subscription with access to Azure AI Foundry in swedencentral

## Quick Start

```bash
azd init -t tdd-azd-demo-builder
cd scenario/ai-3026-smart-home
azd up
```

## Project Structure

```text
scenario/ai-3026-smart-home/
├── azure.yaml              # azd service definition
├── infra/
│   ├── main.bicep          # Main infrastructure template
│   ├── main.bicepparam     # Parameters file
│   └── modules/            # Bicep modules (AI, App Service, etc.)
├── src/
│   └── SmartHome.Web/      # .NET 10 Razor Pages application
└── demoguide/
    ├── demoguide.md        # Step-by-step demo walkthrough
    └── images/             # Demo screenshots
```

## NuGet Packages

| Package                    | Version | Purpose                               |
| -------------------------- | ------- | ------------------------------------- |
| Azure.AI.Projects          | 2.0.0   | AIProjectClient, project operations   |
| Azure.AI.Projects.Agents   | 2.0.0   | DeclarativeAgentDefinition, admin API |
| Azure.AI.Extensions.OpenAI | 2.0.0   | ProjectResponsesClient, FunctionTool  |

## License

MIT

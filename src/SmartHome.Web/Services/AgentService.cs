using System.Text.Json;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Extensions.OpenAI;
using OpenAI.Responses;
using SmartHome.Web.Models;

#pragma warning disable OPENAI001

namespace SmartHome.Web.Services;

public class AgentService
{
    private readonly AIProjectClient _projectClient;
    private readonly SmartHomeToolService _toolService;
    private readonly ILogger<AgentService> _logger;
    private ProjectsAgentVersion? _agent;

    private const string AgentName = "SmartHomeAgent";
    private const string ModelDeployment = "gpt-4o";
    private const string SystemPrompt = """
        You are a smart home assistant controlling lights in a house with 5 rooms:
        Kitchen, Living Room, Bedroom, Bathroom, and Garage. Your job is to anticipate 
        the lighting needs of your users.
        
        Use the provided tools to check the status of lights and turn them on or off.
        Always confirm the action you took and the current state.
        Be friendly, concise, and helpful.
        If a user asks about something unrelated to lights, politely redirect them.
        When reporting status, use a clear format showing each room and whether its light is ON or OFF.
        """;

    private static readonly FunctionTool GetAllLightStatusTool = ResponseTool.CreateFunctionTool(
        functionName: "get_all_light_status",
        functionDescription: "Get the on/off status of all room lights in the house",
        functionParameters: BinaryData.FromString("{}"),
        strictModeEnabled: false
    );

    private static readonly FunctionTool GetRoomLightStatusTool = ResponseTool.CreateFunctionTool(
        functionName: "get_room_light_status",
        functionDescription: "Get the light status for a specific room",
        functionParameters: BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Room_name = new { Type = "string", Description = "Name of the room (Kitchen, Living Room, Bedroom, Bathroom, Garage)" }
                },
                Required = new[] { "room_name" }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        ),
        strictModeEnabled: false
    );

    private static readonly FunctionTool TurnLightOnTool = ResponseTool.CreateFunctionTool(
        functionName: "turn_light_on",
        functionDescription: "Turn on the light in a specific room",
        functionParameters: BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Room_name = new { Type = "string", Description = "Name of the room to turn the light on" }
                },
                Required = new[] { "room_name" }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        ),
        strictModeEnabled: false
    );

    private static readonly FunctionTool TurnLightOffTool = ResponseTool.CreateFunctionTool(
        functionName: "turn_light_off",
        functionDescription: "Turn off the light in a specific room",
        functionParameters: BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Room_name = new { Type = "string", Description = "Name of the room to turn the light off" }
                },
                Required = new[] { "room_name" }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        ),
        strictModeEnabled: false
    );

    public AgentService(AIProjectClient projectClient, SmartHomeToolService toolService, ILogger<AgentService> logger)
    {
        _projectClient = projectClient;
        _toolService = toolService;
        _logger = logger;
    }

    public async Task EnsureAgentAsync()
    {
        if (_agent != null) return;

        try
        {
            var definition = new DeclarativeAgentDefinition(model: ModelDeployment)
            {
                Instructions = SystemPrompt,
                Tools = { GetAllLightStatusTool, GetRoomLightStatusTool, TurnLightOnTool, TurnLightOffTool }
            };

            _agent = await _projectClient.AgentAdministrationClient.CreateAgentVersionAsync(
                agentName: AgentName,
                options: new(definition));

            _logger.LogInformation("Created agent version: {Name} v{Version}",
                _agent.Name, _agent.Version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize agent — will retry on next request");
        }
    }

    public async Task<(string Text, string ResponseId)> ChatAsync(string userMessage, string? previousResponseId, HouseState state)
    {
        if (_agent == null)
            await EnsureAgentAsync();

        if (_agent == null)
            return ("The AI agent is not available yet. Please try again in a moment.", previousResponseId ?? "");

        var responsesClient = _projectClient.ProjectOpenAIClient
            .GetProjectResponsesClientForAgent(_agent.Name);

        _logger.LogInformation("Sending message to agent. PreviousResponseId={PreviousResponseId}", previousResponseId);

        var inputItems = new List<ResponseItem>
        {
            ResponseItem.CreateUserMessageItem(userMessage)
        };

        ResponseResult response = await responsesClient.CreateResponseAsync(
            previousResponseId: previousResponseId,
            inputItems: inputItems);

        // Handle function tool calls in a loop
        bool functionCalled;
        const int maxRounds = 10;
        int round = 0;
        do
        {
            previousResponseId = response.Id;
            inputItems.Clear();
            functionCalled = false;

            foreach (var outputItem in response.OutputItems)
            {
                inputItems.Add(outputItem);
                if (outputItem is FunctionCallResponseItem functionCall)
                {
                    _logger.LogInformation("Executing tool: {Tool}({Args})",
                        functionCall.FunctionName, functionCall.FunctionArguments);
                    var result = ExecuteToolCall(functionCall.FunctionName, functionCall.FunctionArguments.ToString(), state);
                    inputItems.Add(ResponseItem.CreateFunctionCallOutputItem(functionCall.CallId, result));
                    functionCalled = true;
                }
            }

            if (functionCalled)
            {
                response = await responsesClient.CreateResponseAsync(
                    previousResponseId: previousResponseId,
                    inputItems: inputItems);
            }
            round++;
        } while (functionCalled && round < maxRounds);

        return (response.GetOutputText() ?? "No response received from the agent.", response.Id);
    }

    private string ExecuteToolCall(string functionName, string arguments, HouseState state)
    {
        using var argsDoc = JsonDocument.Parse(arguments);

        return functionName switch
        {
            "get_all_light_status" => _toolService.GetAllLightStatus(state),
            "get_room_light_status" => _toolService.GetRoomLightStatus(state, argsDoc.RootElement.GetProperty("room_name").GetString()!),
            "turn_light_on" => _toolService.TurnLightOn(state, argsDoc.RootElement.GetProperty("room_name").GetString()!),
            "turn_light_off" => _toolService.TurnLightOff(state, argsDoc.RootElement.GetProperty("room_name").GetString()!),
            _ => JsonSerializer.Serialize(new { Error = $"Unknown function: {functionName}" })
        };
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartHome.Web.Models;
using SmartHome.Web.Services;

namespace SmartHome.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AgentService _agentService;
    private readonly ILogger<IndexModel> _logger;

    public HouseState HouseState { get; set; } = new();
    public List<ChatMessage> ChatMessages { get; set; } = [];

    [BindProperty]
    public string? UserMessage { get; set; }

    public IndexModel(AgentService agentService, ILogger<IndexModel> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    public void OnGet()
    {
        LoadSessionState();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadSessionState();

        if (string.IsNullOrWhiteSpace(UserMessage))
            return Page();

        ChatMessages.Add(new ChatMessage { Role = "user", Content = UserMessage });

        try
        {
            var previousResponseId = HttpContext.Session.GetString("PreviousResponseId");

            _logger.LogInformation("Sending chat message to agent. PreviousResponseId={PreviousResponseId}", previousResponseId);
            var (response, newResponseId) = await _agentService.ChatAsync(UserMessage, previousResponseId, HouseState);
            HttpContext.Session.SetString("PreviousResponseId", newResponseId);
            ChatMessages.Add(new ChatMessage { Role = "assistant", Content = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent chat failed");
            ChatMessages.Add(new ChatMessage { Role = "assistant", Content = $"Error: {ex.Message}" });
        }

        SaveSessionState();

        UserMessage = string.Empty;
        return Page();
    }

    private void LoadSessionState()
    {
        var houseJson = HttpContext.Session.GetString("HouseState");
        if (houseJson != null)
            HouseState = JsonSerializer.Deserialize<HouseState>(houseJson) ?? new HouseState();

        var chatJson = HttpContext.Session.GetString("ChatMessages");
        if (chatJson != null)
            ChatMessages = JsonSerializer.Deserialize<List<ChatMessage>>(chatJson) ?? [];
    }

    private void SaveSessionState()
    {
        HttpContext.Session.SetString("HouseState", JsonSerializer.Serialize(HouseState));
        HttpContext.Session.SetString("ChatMessages", JsonSerializer.Serialize(ChatMessages));
    }
}

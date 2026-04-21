using System.Text.Json;
using SmartHome.Web.Models;

namespace SmartHome.Web.Services;

public class SmartHomeToolService
{
    private static readonly string[] ValidRooms = ["Kitchen", "Living Room", "Bedroom", "Bathroom", "Garage"];

    public string GetAllLightStatus(HouseState state)
    {
        var status = state.Rooms.Select(r => new { Room = r.Key, Status = r.Value ? "ON" : "OFF" });
        return JsonSerializer.Serialize(status);
    }

    public string GetRoomLightStatus(HouseState state, string roomName)
    {
        var normalized = NormalizeRoomName(roomName);
        if (normalized == null)
            return JsonSerializer.Serialize(new { Error = $"Unknown room: {roomName}. Valid rooms: {string.Join(", ", ValidRooms)}" });

        var isOn = state.Rooms[normalized];
        return JsonSerializer.Serialize(new { Room = normalized, Status = isOn ? "ON" : "OFF" });
    }

    public string TurnLightOn(HouseState state, string roomName)
    {
        var normalized = NormalizeRoomName(roomName);
        if (normalized == null)
            return JsonSerializer.Serialize(new { Error = $"Unknown room: {roomName}. Valid rooms: {string.Join(", ", ValidRooms)}" });

        state.Rooms[normalized] = true;
        return JsonSerializer.Serialize(new { Room = normalized, Status = "ON", Message = $"{normalized} light turned ON", AllRooms = state.Rooms.Select(r => new { Room = r.Key, Status = r.Value ? "ON" : "OFF" }) });
    }

    public string TurnLightOff(HouseState state, string roomName)
    {
        var normalized = NormalizeRoomName(roomName);
        if (normalized == null)
            return JsonSerializer.Serialize(new { Error = $"Unknown room: {roomName}. Valid rooms: {string.Join(", ", ValidRooms)}" });

        state.Rooms[normalized] = false;
        return JsonSerializer.Serialize(new { Room = normalized, Status = "OFF", Message = $"{normalized} light turned OFF", AllRooms = state.Rooms.Select(r => new { Room = r.Key, Status = r.Value ? "ON" : "OFF" }) });
    }

    private static string? NormalizeRoomName(string input)
    {
        return ValidRooms.FirstOrDefault(r =>
            r.Equals(input, StringComparison.OrdinalIgnoreCase) ||
            r.Replace(" ", "").Equals(input.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
    }
}

namespace SmartHome.Web.Models;

public class HouseState
{
    public Dictionary<string, bool> Rooms { get; set; } = new()
    {
        ["Kitchen"] = false,
        ["Living Room"] = false,
        ["Bedroom"] = false,
        ["Bathroom"] = false,
        ["Garage"] = false
    };

    public List<Room> GetRoomList() =>
        Rooms.Select(r => new Room
        {
            Name = r.Key,
            IsLightOn = r.Value,
            SvgId = r.Key.Replace(" ", "").ToLowerInvariant()
        }).ToList();
}

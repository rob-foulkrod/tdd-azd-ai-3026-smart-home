namespace SmartHome.Web.Models;

public class Room
{
    public required string Name { get; set; }
    public bool IsLightOn { get; set; }
    public required string SvgId { get; set; }
}

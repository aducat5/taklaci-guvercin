using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Shared.DTOs;

public class BirdDnaDto
{
    public GeneDto PrimaryColor { get; set; } = new();
    public GeneDto SecondaryColor { get; set; } = new();
    public GeneDto Pattern { get; set; } = new();
    public GeneDto TailType { get; set; } = new();
    public GeneDto CrestType { get; set; } = new();
    public ElementDto Element { get; set; }
}

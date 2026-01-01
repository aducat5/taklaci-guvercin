namespace TaklaciGuvercin.Shared.DTOs;

public class GeneDto
{
    public string TraitName { get; set; } = string.Empty;
    public string ExpressedAllele { get; set; } = string.Empty;
    public bool IsDominant { get; set; }
}

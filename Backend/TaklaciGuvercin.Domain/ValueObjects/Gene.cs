using TaklaciGuvercin.Domain.Enums;

namespace TaklaciGuvercin.Domain.ValueObjects;

public class Gene
{
    public string TraitName { get; private set; } = string.Empty;
    public string Allele1 { get; private set; } = string.Empty;
    public string Allele2 { get; private set; } = string.Empty;
    public GeneType Type { get; private set; }

    private Gene() { }

    public static Gene Create(string traitName, string allele1, string allele2, GeneType type)
    {
        return new Gene
        {
            TraitName = traitName,
            Allele1 = allele1,
            Allele2 = allele2,
            Type = type
        };
    }

    public bool IsDominant => Type == GeneType.Dominant;

    public bool IsHomozygous => Allele1 == Allele2;

    public bool IsHeterozygous => Allele1 != Allele2;

    public string GetExpressedAllele()
    {
        if (IsHomozygous) return Allele1;
        return IsDominant ? Allele1 : Allele2;
    }
}

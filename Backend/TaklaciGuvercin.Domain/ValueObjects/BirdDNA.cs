using TaklaciGuvercin.Domain.Enums;

namespace TaklaciGuvercin.Domain.ValueObjects;

public class BirdDNA
{
    // Visual Genes (Phenotype)
    public Gene PrimaryColor { get; private set; } = null!;
    public Gene SecondaryColor { get; private set; } = null!;
    public Gene Pattern { get; private set; } = null!;
    public Gene TailType { get; private set; } = null!;
    public Gene CrestType { get; private set; } = null!;

    // Element Gene
    public Gene ElementGene { get; private set; } = null!;
    public Element Element { get; private set; }

    // Hidden Genes (Genotype - not visible but affects breeding)
    public Gene HiddenTrait1 { get; private set; } = null!;
    public Gene HiddenTrait2 { get; private set; } = null!;

    // Mutation chance modifier
    public float MutationFactor { get; private set; }

    private BirdDNA() { }

    public static BirdDNA Create(
        Gene primaryColor,
        Gene secondaryColor,
        Gene pattern,
        Gene tailType,
        Gene crestType,
        Gene elementGene,
        Element element,
        Gene hiddenTrait1,
        Gene hiddenTrait2,
        float mutationFactor = 0.05f)
    {
        return new BirdDNA
        {
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            Pattern = pattern,
            TailType = tailType,
            CrestType = crestType,
            ElementGene = elementGene,
            Element = element,
            HiddenTrait1 = hiddenTrait1,
            HiddenTrait2 = hiddenTrait2,
            MutationFactor = Math.Clamp(mutationFactor, 0f, 1f)
        };
    }

    public static BirdDNA CreateDefault()
    {
        return new BirdDNA
        {
            PrimaryColor = Gene.Create("PrimaryColor", "Gray", "Gray", GeneType.Dominant),
            SecondaryColor = Gene.Create("SecondaryColor", "White", "White", GeneType.Recessive),
            Pattern = Gene.Create("Pattern", "Solid", "Solid", GeneType.Dominant),
            TailType = Gene.Create("TailType", "Normal", "Normal", GeneType.Dominant),
            CrestType = Gene.Create("CrestType", "None", "None", GeneType.Recessive),
            ElementGene = Gene.Create("Element", "None", "None", GeneType.Recessive),
            Element = Element.None,
            HiddenTrait1 = Gene.Create("Hidden1", "Normal", "Normal", GeneType.Recessive),
            HiddenTrait2 = Gene.Create("Hidden2", "Normal", "Normal", GeneType.Recessive),
            MutationFactor = 0.05f
        };
    }

    public IEnumerable<Gene> GetAllGenes()
    {
        yield return PrimaryColor;
        yield return SecondaryColor;
        yield return Pattern;
        yield return TailType;
        yield return CrestType;
        yield return ElementGene;
        yield return HiddenTrait1;
        yield return HiddenTrait2;
    }
}

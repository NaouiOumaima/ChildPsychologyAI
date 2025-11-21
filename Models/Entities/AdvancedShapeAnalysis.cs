namespace ChildPsychologyAI.Models.Entities;

public class AdvancedShapeAnalysis
{
    public List<DetectedShape> Shapes { get; set; } = new();
    public List<DetectedSymbol> Symbols { get; set; } = new();
    public CompositionAnalysis Composition { get; set; } = new();
    public List<string> PsychologicalIndicators { get; set; } = new();
}

public class DetectedShape
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageSize { get; set; }
    public string Position { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class DetectedSymbol
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Complexity { get; set; } = string.Empty;
    public List<string> Characteristics { get; set; } = new();
}

public class CompositionAnalysis
{
    public string Balance { get; set; } = string.Empty;
    public string SpaceUsage { get; set; } = string.Empty;
    public string Pressure { get; set; } = string.Empty;
    public List<string> CompositionIndicators { get; set; } = new();
}
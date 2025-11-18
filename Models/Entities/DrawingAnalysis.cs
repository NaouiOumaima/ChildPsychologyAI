using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Models.Entities;

public class DrawingAnalysis : EntityBase
{
    public string ChildId { get; set; } = string.Empty;
    public string DrawingUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime DrawingDate { get; set; }
    public ColorAnalysis Colors { get; set; } = new();
    public ShapeAnalysis Shapes { get; set; } = new();
    public EmotionalAnalysis Emotions { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
}

public class ColorAnalysis
{
    public Dictionary<string, double> ColorDistribution { get; set; } = new();
    public string DominantColor { get; set; } = string.Empty;
    public double ColorIntensity { get; set; }
    public List<string> ColorInterpretations { get; set; } = new();
}

public class ShapeAnalysis
{
    public List<string> DetectedShapes { get; set; } = new();
    public Dictionary<string, int> ShapeFrequency { get; set; } = new();
    public List<string> SymbolInterpretations { get; set; } = new();
}

public class EmotionalAnalysis
{
    public Dictionary<string, double> EmotionProbabilities { get; set; } = new();
    public string PrimaryEmotion { get; set; } = string.Empty;
    public List<string> EmotionalIndicators { get; set; } = new();
}
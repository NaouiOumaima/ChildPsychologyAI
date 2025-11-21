namespace ChildPsychologyAI.Models.Entities;

public class AdvancedEmotionalAnalysis
{
    public Dictionary<string, double> EmotionScores { get; set; } = new();
    public string DominantEmotion { get; set; } = string.Empty;
    public string EmotionalState { get; set; } = string.Empty;
    public List<EmotionalIndicator> Indicators { get; set; } = new();
    public RiskAssessment RiskAssessment { get; set; } = new();
}

public class EmotionalIndicator
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string EmotionalCorrelation { get; set; } = string.Empty;
}

public class RiskAssessment
{
    public string Level { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public bool RequiresAttention { get; set; }
}
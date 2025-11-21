using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Models.Entities;

public class CompleteDrawingAnalysis : EntityBase
{
    public string ChildId { get; set; } = string.Empty;
    public string DrawingUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime DrawingDate { get; set; }

    // Analyses de base (existantes)
    public ColorAnalysis Colors { get; set; } = new();
    public ShapeAnalysis Shapes { get; set; } = new();
    public EmotionalAnalysis Emotions { get; set; } = new();

    // Nouvelles analyses avancées
    public AdvancedShapeAnalysis AdvancedShapes { get; set; } = new();
    public AdvancedEmotionalAnalysis AdvancedEmotions { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public string AnalysisType { get; set; } = "complete"; // "basic" ou "complete"
}
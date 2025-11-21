using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Interfaces;

public interface IAdvancedEmotionalAnalysisService
{
    Task<AdvancedEmotionalAnalysis> AnalyzeEmotionsAsync(ColorAnalysis colors, AdvancedShapeAnalysis shapes);
    Task<RiskAssessment> AssessRisksAsync(AdvancedEmotionalAnalysis emotions);
}
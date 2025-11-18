using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Interfaces;

public interface IDrawingAnalysisService
{
    Task<DrawingAnalysis> AnalyzeDrawingAsync(string imagePath, string childId);
    Task<List<DrawingAnalysis>> GetAnalysesByChildIdAsync(string childId);
    Task<DrawingAnalysis?> GetAnalysisByIdAsync(string analysisId);
}
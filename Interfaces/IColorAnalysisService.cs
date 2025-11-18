using ChildPsychologyAI.Models.Entities;
using OpenCvSharp;

namespace ChildPsychologyAI.Interfaces;

public interface IColorAnalysisService
{
    Task<ColorAnalysis> AnalyzeColorsAsync(Mat image);
}
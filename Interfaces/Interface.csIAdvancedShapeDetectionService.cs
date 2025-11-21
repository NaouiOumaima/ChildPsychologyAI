using ChildPsychologyAI.Models.Entities;
using OpenCvSharp;

namespace ChildPsychologyAI.Interfaces;

public interface IAdvancedShapeDetectionService
{
    Task<AdvancedShapeAnalysis> AnalyzeShapesAsync(Mat image);
    Task<List<Contour>> DetectContoursAsync(Mat image);
    Task<string> ClassifyShapeAsync(Contour contour);
}
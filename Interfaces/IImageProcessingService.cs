using OpenCvSharp;

namespace ChildPsychologyAI.Interfaces;

public interface IImageProcessingService
{
    Task<Mat> LoadImageAsync(string imagePath);
    Task<Mat> PreprocessImageAsync(Mat image);
    Task<List<Contour>> ExtractContoursAsync(Mat image);
}

public record Contour(Point[] Points)
{
    public double Area => Cv2.ContourArea(Points);
    public Rect BoundingRect => Cv2.BoundingRect(Points);
};
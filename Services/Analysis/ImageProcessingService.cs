using OpenCvSharp;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Services.Analysis;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<Mat> LoadImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            var image = Cv2.ImRead(imagePath, ImreadModes.Color);
            if (image.Empty())
                throw new ArgumentException($"Impossible de charger l'image: {imagePath}");

            return image;
        });
    }

    public async Task<Mat> PreprocessImageAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            // Réduction du bruit
            var processed = new Mat();
            Cv2.GaussianBlur(image, processed, new Size(5, 5), 0);

            // Amélioration du contraste
            Cv2.ConvertScaleAbs(processed, processed, 1.2, 0);

            return processed;
        });
    }

    public async Task<List<Contour>> ExtractContoursAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            var gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            var edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);

            Cv2.FindContours(edges, out var contours, out var hierarchy,
                RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            return contours.Select(c => new Contour(c)).ToList();
        });
    }
}
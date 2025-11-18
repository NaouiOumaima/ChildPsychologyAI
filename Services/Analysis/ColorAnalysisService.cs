using OpenCvSharp;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Services.Analysis;

public class ColorAnalysisService : IColorAnalysisService
{
    private static readonly Dictionary<string, Scalar> ColorRanges = new()
    {
        ["red"] = new Scalar(0, 0, 100),
        ["blue"] = new Scalar(100, 0, 0),
        ["green"] = new Scalar(0, 100, 0),
        ["yellow"] = new Scalar(0, 255, 255),
        ["black"] = new Scalar(0, 0, 0),
        ["white"] = new Scalar(255, 255, 255),
        ["purple"] = new Scalar(128, 0, 128),
        ["orange"] = new Scalar(0, 165, 255),
        ["brown"] = new Scalar(42, 42, 165)
    };

    public async Task<ColorAnalysis> AnalyzeColorsAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            var colorDistribution = new Dictionary<string, double>();
            var totalPixels = image.Width * image.Height;

            // Convertir en HSV pour une meilleure analyse des couleurs
            var hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

            foreach (var (colorName, range) in ColorRanges)
            {
                var mask = CreateColorMask(hsv, colorName);
                var percentage = (Cv2.CountNonZero(mask) / (double)totalPixels) * 100;

                if (percentage > 1.0) // Ignorer les couleurs < 1%
                    colorDistribution[colorName] = Math.Round(percentage, 2);
            }

            var dominantColor = colorDistribution.OrderByDescending(x => x.Value).FirstOrDefault();
            var intensity = CalculateColorIntensity(image);
            var interpretations = InterpretColors(colorDistribution);

            return new ColorAnalysis
            {
                ColorDistribution = colorDistribution,
                DominantColor = dominantColor.Key ?? "unknown",
                ColorIntensity = intensity,
                ColorInterpretations = interpretations
            };
        });
    }

    private Mat CreateColorMask(Mat hsv, string colorName)
    {
        var mask = new Mat();
        var (lower, upper) = GetColorRange(colorName);

        Cv2.InRange(hsv, lower, upper, mask);
        return mask;
    }

    private (Scalar lower, Scalar upper) GetColorRange(string colorName)
    {
        return colorName.ToLower() switch
        {
            "red" => (new Scalar(0, 120, 70), new Scalar(10, 255, 255)),
            "blue" => (new Scalar(100, 150, 0), new Scalar(140, 255, 255)),
            "green" => (new Scalar(40, 40, 40), new Scalar(80, 255, 255)),
            "yellow" => (new Scalar(20, 100, 100), new Scalar(30, 255, 255)),
            _ => (new Scalar(0, 0, 0), new Scalar(180, 255, 255))
        };
    }

    private double CalculateColorIntensity(Mat image)
    {
        var lab = new Mat();
        Cv2.CvtColor(image, lab, ColorConversionCodes.BGR2Lab);

        Cv2.Split(lab, out var channels);
        var lightness = channels[0];

        return lightness.Mean().Val0 / 255.0;
    }

    private List<string> InterpretColors(Dictionary<string, double> colorDistribution)
    {
        var interpretations = new List<string>();

        foreach (var (color, percentage) in colorDistribution)
        {
            if (percentage > 30)
            {
                interpretations.Add(GetColorInterpretation(color, percentage));
            }
        }

        return interpretations;
    }

    private string GetColorInterpretation(string color, double percentage)
    {
        return color.ToLower() switch
        {
            "red" => percentage > 50 ?
                "Utilisation importante de rouge: peut indiquer de la colère ou une forte énergie" :
                "Présence de rouge: énergie, passion",

            "blue" => "Couleur bleue: calme, sérénité, ou parfois tristesse",
            "black" => percentage > 20 ?
                "Utilisation importante de noir: possible anxiété ou tristesse" :
                "Présence de noir: peut indiquer de l'anxiété",

            "yellow" => "Couleur jaune: joie, optimisme, énergie positive",
            "green" => "Couleur verte: équilibre, croissance, stabilité",
            _ => $"Couleur {color}: signification à analyser dans le contexte"
        };
    }
}
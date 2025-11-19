using OpenCvSharp;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Services.Analysis;

public class ColorAnalysisService : IColorAnalysisService
{
    private static readonly Dictionary<string, (Scalar lower, Scalar upper)> ColorRanges = new()
    {
        // Format HSV: (Hue, Saturation, Value)
        ["red"] = (new Scalar(0, 100, 100), new Scalar(10, 255, 255)),
        ["red2"] = (new Scalar(170, 100, 100), new Scalar(180, 255, 255)), // Rouge a deux plages
        ["orange"] = (new Scalar(10, 100, 100), new Scalar(20, 255, 255)),
        ["yellow"] = (new Scalar(20, 100, 100), new Scalar(30, 255, 255)),
        ["green"] = (new Scalar(40, 100, 100), new Scalar(80, 255, 255)),
        ["blue"] = (new Scalar(100, 100, 100), new Scalar(130, 255, 255)),
        ["purple"] = (new Scalar(130, 100, 100), new Scalar(170, 255, 255)),
        ["pink"] = (new Scalar(140, 50, 100), new Scalar(170, 255, 255)),
        ["brown"] = (new Scalar(10, 100, 50), new Scalar(20, 255, 150)),
        ["black"] = (new Scalar(0, 0, 0), new Scalar(180, 255, 50)),      // Valeur basse
        ["white"] = (new Scalar(0, 0, 200), new Scalar(180, 50, 255)),    // Saturation basse
        ["gray"] = (new Scalar(0, 0, 50), new Scalar(180, 50, 200))
    };

    public async Task<ColorAnalysis> AnalyzeColorsAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            var colorDistribution = new Dictionary<string, double>();
            var totalPixels = image.Width * image.Height;

            if (totalPixels == 0)
                return new ColorAnalysis();

            // Convertir en HSV
            var hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

            // Masque pour éviter les doublons
            var usedPixels = new Mat(hsv.Size(), MatType.CV_8UC1, Scalar.All(0));

            foreach (var (colorName, range) in ColorRanges)
            {
                var mask = new Mat();
                Cv2.InRange(hsv, range.lower, range.upper, mask);

                // Exclure les pixels déjà comptés
                var exclusiveMask = new Mat();
                Cv2.BitwiseAnd(mask, usedPixels, exclusiveMask);
                Cv2.Subtract(mask, exclusiveMask, mask);

                var pixelCount = Cv2.CountNonZero(mask);
                var percentage = (pixelCount / (double)totalPixels) * 100;

                if (percentage > 0.5) // Seuil à 0.5% pour éviter le bruit
                {
                    colorDistribution[colorName] = Math.Round(percentage, 2);

                    // Marquer ces pixels comme utilisés
                    Cv2.BitwiseOr(usedPixels, mask, usedPixels);
                }
            }

            // Gérer les pixels restants comme "other"
            var remainingPixels = totalPixels - Cv2.CountNonZero(usedPixels);
            var remainingPercentage = (remainingPixels / (double)totalPixels) * 100;

            if (remainingPercentage > 0.5)
            {
                colorDistribution["other"] = Math.Round(remainingPercentage, 2);
            }

            // Fusionner red et red2
            if (colorDistribution.ContainsKey("red2"))
            {
                colorDistribution["red"] = colorDistribution.GetValueOrDefault("red", 0) + colorDistribution["red2"];
                colorDistribution.Remove("red2");
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

    private double CalculateColorIntensity(Mat image)
    {
        var hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

        Cv2.Split(hsv, out var channels);
        var saturation = channels[1];
        var value = channels[2];

        var avgSaturation = saturation.Mean().Val0 / 255.0;
        var avgValue = value.Mean().Val0 / 255.0;

        return (avgSaturation + avgValue) / 2.0;
    }

    private List<string> InterpretColors(Dictionary<string, double> colorDistribution)
    {
        var interpretations = new List<string>();

        // Prendre seulement les 3 couleurs principales
        var topColors = colorDistribution
            .OrderByDescending(x => x.Value)
            .Take(3)
            .ToList();

        foreach (var (color, percentage) in topColors)
        {
            interpretations.Add(GetColorInterpretation(color, percentage));
        }

        // Analyse globale
        if (colorDistribution.ContainsKey("black") && colorDistribution["black"] > 50)
            interpretations.Add("Prédominance de noir: nécessite attention particulière");
        else if (colorDistribution.Count == 1)
            interpretations.Add("Dessin monochromatique: expression focalisée");
        else if (colorDistribution.Count >= 5)
            interpretations.Add("Palette colorée variée: expression émotionnelle riche");

        return interpretations;
    }

    private string GetColorInterpretation(string color, double percentage)
    {
        return color.ToLower() switch
        {
            "red" => percentage > 30 ?
                "Rouge dominant: énergie intense, passion" :
                "Présence de rouge: énergie, vitalité",

            "blue" => percentage > 30 ?
                "Bleu dominant: calme, sérénité" :
                "Présence de bleu: paix, tranquillité",

            "black" => percentage > 20 ?
                "Noir important: possible anxiété ou tristesse" :
                "Traces de noir: peut indiquer de l'anxiété",

            "white" => percentage > 30 ?
                "Blanc dominant: pureté ou vide émotionnel" :
                "Présence de blanc: clarté, innocence",

            "yellow" => "Jaune: joie, optimisme",
            "green" => "Vert: équilibre, croissance",
            "orange" => "Orange: créativité, enthousiasme",
            "purple" => "Violet: imagination, spiritualité",
            "pink" => "Rose: tendresse, affection",
            "brown" => "Marron: stabilité, sécurité",
            "gray" => "Gris: neutralité, maturité",
            "other" => "Couleurs variées: palette complexe",
            _ => $"Couleur {color}: signification contextuelle"
        };
    }
}
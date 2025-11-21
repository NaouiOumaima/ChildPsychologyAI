using OpenCvSharp;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Services.Analysis;

public class AdvancedShapeDetectionService : IAdvancedShapeDetectionService
{
    private readonly ILogger<AdvancedShapeDetectionService> _logger;

    public AdvancedShapeDetectionService(ILogger<AdvancedShapeDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedShapeAnalysis> AnalyzeShapesAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            var analysis = new AdvancedShapeAnalysis();

            try
            {
                var contours = DetectContoursAsync(image).Result;

                var shapes = new List<DetectedShape>();
                foreach (var contour in contours.Where(c => c.Area > 100))
                {
                    var shapeType = ClassifyShapeAsync(contour).Result;
                    var shapePosition = AnalyzeShapePosition(contour, image);

                    shapes.Add(new DetectedShape
                    {
                        Type = shapeType,
                        Count = 1,
                        AverageSize = contour.Area,
                        Position = shapePosition,
                        Confidence = CalculateShapeConfidence(contour, shapeType)
                    });
                }

                analysis.Shapes = shapes
                    .GroupBy(s => s.Type)
                    .Select(g => new DetectedShape
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        AverageSize = g.Average(s => s.AverageSize),
                        Position = GetDominantPosition(g.ToList()),
                        Confidence = g.Average(s => s.Confidence)
                    })
                    .ToList();

                analysis.Symbols = DetectSymbols(shapes, image);
                analysis.Composition = AnalyzeComposition(shapes, image);
                analysis.PsychologicalIndicators = GeneratePsychologicalIndicators(analysis);

                _logger.LogInformation($"Analyse avancée: {analysis.Shapes.Count} formes, {analysis.Symbols.Count} symboles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse des formes avancées");
            }

            return analysis;
        });
    }

    public async Task<List<Contour>> DetectContoursAsync(Mat image)
    {
        return await Task.Run(() =>
        {
            try
            {
                var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                var blurred = new Mat();
                Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

                var edges = new Mat();
                Cv2.Canny(blurred, edges, 50, 150);

                Cv2.FindContours(edges, out var contours, out var hierarchy,
                    RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                return contours.Select(c => new Contour(c)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la détection des contours");
                return new List<Contour>();
            }
        });
    }

    public async Task<string> ClassifyShapeAsync(Contour contour)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (contour.Area < 100) return "noise";

                var hull = Cv2.ConvexHull(contour.Points);
                var hullArea = Cv2.ContourArea(hull);
                var solidity = contour.Area / hullArea;
                var aspectRatio = contour.BoundingRect.Width / (double)contour.BoundingRect.Height;

                var epsilon = 0.02 * Cv2.ArcLength(contour.Points, true);
                var approx = Cv2.ApproxPolyDP(contour.Points, epsilon, true);

                if (approx.Length == 3) return "triangle";
                if (approx.Length == 4)
                    return Math.Abs(aspectRatio - 1.0) < 0.2 ? "square" : "rectangle";
                if (approx.Length > 8 && solidity > 0.8)
                    return Math.Abs(aspectRatio - 1.0) < 0.2 ? "circle" : "ellipse";

                return solidity > 0.9 ? "blob" : "organic";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la classification de forme");
                return "unknown";
            }
        });
    }

    private string AnalyzeShapePosition(Contour contour, Mat image)
    {
        try
        {
            var center = new Point2f(
                contour.BoundingRect.X + contour.BoundingRect.Width / 2.0f,
                contour.BoundingRect.Y + contour.BoundingRect.Height / 2.0f
            );

            var imageCenter = new Point2f(image.Width / 2.0f, image.Height / 2.0f);

            if (center.X < imageCenter.X - image.Width * 0.25) return "left";
            if (center.X > imageCenter.X + image.Width * 0.25) return "right";
            if (center.Y < imageCenter.Y - image.Height * 0.25) return "top";
            if (center.Y > imageCenter.Y + image.Height * 0.25) return "bottom";

            return "center";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse de position");
            return "unknown";
        }
    }

    private double CalculateShapeConfidence(Contour contour, string shapeType)
    {
        try
        {
            var hull = Cv2.ConvexHull(contour.Points);
            var hullArea = Cv2.ContourArea(hull);
            var solidity = contour.Area / hullArea;

            return shapeType switch
            {
                "circle" or "ellipse" => solidity > 0.8 ? 0.9 : 0.6,
                "square" or "rectangle" => solidity > 0.85 ? 0.8 : 0.5,
                "triangle" => solidity > 0.75 ? 0.7 : 0.4,
                _ => 0.3
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de confiance");
            return 0.1;
        }
    }

    private string GetDominantPosition(List<DetectedShape> shapes)
    {
        try
        {
            return shapes.Any()
                ? shapes.GroupBy(s => s.Position).OrderByDescending(g => g.Count()).First().Key
                : "unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détermination de la position dominante");
            return "unknown";
        }
    }

    private List<DetectedSymbol> DetectSymbols(List<DetectedShape> shapes, Mat image)
    {
        var symbols = new List<DetectedSymbol>();

        try
        {
            var circles = shapes.Where(s => s.Type == "circle").ToList();
            var squares = shapes.Where(s => s.Type == "square" || s.Type == "rectangle").ToList();
            var triangles = shapes.Where(s => s.Type == "triangle").ToList();
            var organic = shapes.Where(s => s.Type == "organic" || s.Type == "blob").ToList();

            // Détection de personnages humains
            if (DetectHumanFigure(shapes, image))
            {
                symbols.Add(new DetectedSymbol
                {
                    Type = "human",
                    Count = 1,
                    Complexity = "detailed",
                    Characteristics = new List<string> { "figure_humaine", "representation_personnelle" }
                });
            }

            // Détection de visages
            if (DetectFace(shapes, image))
            {
                symbols.Add(new DetectedSymbol
                {
                    Type = "face",
                    Count = 1,
                    Complexity = "organic",
                    Characteristics = new List<string> { "visage", "expression", "identite" }
                });
            }

            // Détection de maisons plus précise
            if (DetectRealHouse(shapes))
            {
                symbols.Add(new DetectedSymbol
                {
                    Type = "house",
                    Count = 1,
                    Complexity = "structured",
                    Characteristics = new List<string> { "habitation", "foyer", "structure" }
                });
            }

            // Détection d'arbres
            if (DetectTree(shapes))
            {
                symbols.Add(new DetectedSymbol
                {
                    Type = "tree",
                    Count = 1,
                    Complexity = "natural",
                    Characteristics = new List<string> { "nature", "croissance", "vie" }
                });
            }

            // Détection d'émotions via la composition
            var emotionalSymbols = DetectEmotionalSymbols(shapes, image);
            symbols.AddRange(emotionalSymbols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection des symboles");
        }

        return symbols;
    }

    private bool DetectHumanFigure(List<DetectedShape> shapes, Mat image)
    {
        try
        {
            var circles = shapes.Where(s => s.Type == "circle" || s.Type == "ellipse").ToList();
            var rectangles = shapes.Where(s => s.Type == "rectangle").ToList();
            var organicShapes = shapes.Where(s => s.Type == "organic" || s.Type == "blob").ToList();

            var hasHead = circles.Any(c => c.AverageSize < image.Width * image.Height * 0.1);
            var hasBody = rectangles.Any(r => r.AverageSize > 500) || organicShapes.Any(o => o.AverageSize > 1000);

            return hasHead && hasBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection de figure humaine");
            return false;
        }
    }

    private bool DetectFace(List<DetectedShape> shapes, Mat image)
    {
        try
        {
            var circles = shapes.Where(s => s.Type == "circle" || s.Type == "ellipse").ToList();
            var smallCircles = circles.Where(c => c.AverageSize < 1000).ToList();
            var mediumCircles = circles.Where(c => c.AverageSize >= 1000 && c.AverageSize < 5000).ToList();

            return mediumCircles.Count >= 1 && smallCircles.Count >= 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection de visage");
            return false;
        }
    }

    private bool DetectRealHouse(List<DetectedShape> shapes)
    {
        try
        {
            var squares = shapes.Where(s => s.Type == "square" || s.Type == "rectangle").ToList();
            var triangles = shapes.Where(s => s.Type == "triangle").ToList();

            var mainStructure = squares.FirstOrDefault(s => s.AverageSize > 2000);
            var roof = triangles.FirstOrDefault(t => t.AverageSize > 500);

            return mainStructure != null && roof != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection de maison");
            return false;
        }
    }

    private bool DetectTree(List<DetectedShape> shapes)
    {
        try
        {
            var trunk = shapes.FirstOrDefault(s => s.Type == "rectangle" && s.AverageSize < 2000);
            var foliage = shapes.FirstOrDefault(s =>
                (s.Type == "circle" || s.Type == "organic" || s.Type == "blob") &&
                s.AverageSize > 1000);

            return trunk != null && foliage != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection d'arbre");
            return false;
        }
    }

    private List<DetectedSymbol> DetectEmotionalSymbols(List<DetectedShape> shapes, Mat image)
    {
        var emotionalSymbols = new List<DetectedSymbol>();

        try
        {
            var composition = AnalyzeComposition(shapes, image);

            if (composition.SpaceUsage == "constricted")
            {
                emotionalSymbols.Add(new DetectedSymbol
                {
                    Type = "isolation",
                    Count = 1,
                    Complexity = "emotional",
                    Characteristics = new List<string> { "repli", "timidite", "protection" }
                });
            }

            if (composition.Balance == "left-heavy")
            {
                emotionalSymbols.Add(new DetectedSymbol
                {
                    Type = "past_focus",
                    Count = 1,
                    Complexity = "emotional",
                    Characteristics = new List<string> { "nostalgie", "attachment", "memoire" }
                });
            }

            var organicShapes = shapes.Where(s => s.Type == "organic" || s.Type == "blob").ToList();
            if (organicShapes.Count > shapes.Count * 0.7)
            {
                emotionalSymbols.Add(new DetectedSymbol
                {
                    Type = "emotional_expression",
                    Count = 1,
                    Complexity = "fluid",
                    Characteristics = new List<string> { "sensibilite", "expressivite", "emotivite" }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection des symboles émotionnels");
        }

        return emotionalSymbols;
    }

    private CompositionAnalysis AnalyzeComposition(List<DetectedShape> shapes, Mat image)
    {
        var composition = new CompositionAnalysis();

        try
        {
            if (!shapes.Any())
            {
                composition.Balance = "empty";
                composition.SpaceUsage = "minimal";
                composition.Pressure = "unknown";
                return composition;
            }

            var leftShapes = shapes.Count(s => s.Position == "left");
            var rightShapes = shapes.Count(s => s.Position == "right");

            if (Math.Abs(leftShapes - rightShapes) <= 1)
                composition.Balance = "balanced";
            else
                composition.Balance = leftShapes > rightShapes ? "left-heavy" : "right-heavy";

            var totalArea = shapes.Sum(s => s.AverageSize);
            var imageArea = image.Width * image.Height;
            var coverage = totalArea / imageArea;

            composition.SpaceUsage = coverage switch
            {
                < 0.1 => "constricted",
                < 0.3 => "normal",
                < 0.6 => "expansive",
                _ => "crowded"
            };

            composition.Pressure = shapes.Average(s => s.AverageSize) switch
            {
                < 500 => "light",
                < 2000 => "medium",
                _ => "heavy"
            };

            composition.CompositionIndicators = GenerateCompositionIndicators(composition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse de composition");
        }

        return composition;
    }

    private List<string> GenerateCompositionIndicators(CompositionAnalysis composition)
    {
        var indicators = new List<string>();

        try
        {
            if (composition.Balance == "left-heavy")
                indicators.Add("Orientation vers le passé/la mère");
            if (composition.Balance == "right-heavy")
                indicators.Add("Orientation vers le futur/le père");
            if (composition.SpaceUsage == "constricted")
                indicators.Add("Expression réservée ou timide");
            if (composition.SpaceUsage == "expansive")
                indicators.Add("Confiance spatiale");
            if (composition.Pressure == "heavy")
                indicators.Add("Intensité dans l'expression");
            if (composition.Pressure == "light")
                indicators.Add("Délicatesse dans le trait");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération des indicateurs de composition");
        }

        return indicators;
    }

    private List<string> GeneratePsychologicalIndicators(AdvancedShapeAnalysis analysis)
    {
        var indicators = new List<string>();

        try
        {
            var circles = analysis.Shapes.Sum(s => s.Type == "circle" ? s.Count : 0);
            var triangles = analysis.Shapes.Sum(s => s.Type == "triangle" ? s.Count : 0);
            var organic = analysis.Shapes.Sum(s => s.Type == "organic" || s.Type == "blob" ? s.Count : 0);
            var geometric = analysis.Shapes.Sum(s => s.Type == "square" || s.Type == "rectangle" || s.Type == "triangle" ? s.Count : 0);

            if (organic > geometric * 2)
            {
                indicators.Add("Expression émotionnelle fluide et organique");
            }

            if (analysis.Symbols.Any(s => s.Type == "human" || s.Type == "face"))
            {
                indicators.Add("Représentation de soi ou des autres");

                if (analysis.Composition.SpaceUsage == "constricted")
                    indicators.Add("Possible timidité ou réserve dans l'expression");
            }

            if (analysis.Symbols.Any(s => s.Type == "isolation"))
            {
                indicators.Add("Tendance au repli ou besoin d'espace personnel");
            }

            if (analysis.Symbols.Any(s => s.Type == "emotional_expression"))
            {
                indicators.Add("Expressivité émotionnelle marquée");
            }

            if (analysis.Composition.Pressure == "heavy")
            {
                indicators.Add("Intensité émotionnelle dans l'expression");
            }
            else if (analysis.Composition.Pressure == "light")
            {
                indicators.Add("Délicatesse et sensibilité dans le trait");
            }

            // Supprimer les interprétations si les symboles correspondants ne sont pas détectés
            if (!analysis.Symbols.Any(s => s.Type == "house"))
            {
                indicators.RemoveAll(i => i.Contains("familiale") || i.Contains("maison"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération des indicateurs psychologiques");
        }

        return indicators;
    }
}
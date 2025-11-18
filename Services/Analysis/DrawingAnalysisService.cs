using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Models.Enums;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Services.Data;

namespace ChildPsychologyAI.Services.Analysis;

public class DrawingAnalysisService : IDrawingAnalysisService
{
    private readonly IImageProcessingService _imageProcessing;
    private readonly IColorAnalysisService _colorAnalysis;
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<DrawingAnalysisService> _logger;

    public DrawingAnalysisService(
        IImageProcessingService imageProcessing,
        IColorAnalysisService colorAnalysis,
        MongoDbContext mongoContext,
        ILogger<DrawingAnalysisService> logger)
    {
        _imageProcessing = imageProcessing;
        _colorAnalysis = colorAnalysis;
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task<DrawingAnalysis> AnalyzeDrawingAsync(string imagePath, string childId)
    {
        try
        {
            _logger.LogInformation("Début de l'analyse du dessin pour l'enfant {ChildId}", childId);

            // Charger et traiter l'image
            var image = await _imageProcessing.LoadImageAsync(imagePath);
            var processedImage = await _imageProcessing.PreprocessImageAsync(image);

            // Analyser les couleurs
            var colorAnalysis = await _colorAnalysis.AnalyzeColorsAsync(processedImage);

            // Pour le moment, on crée des analyses de formes et émotions factices
            var shapeAnalysis = new ShapeAnalysis
            {
                DetectedShapes = new List<string> { "cercle", "carré", "ligne" },
                ShapeFrequency = new Dictionary<string, int> { { "cercle", 2 }, { "carré", 1 } },
                SymbolInterpretations = new List<string> { "Formes basiques détectées" }
            };

            var emotionalAnalysis = new EmotionalAnalysis
            {
                EmotionProbabilities = new Dictionary<string, double>
                {
                    { "joy", 0.6 },
                    { "sadness", 0.1 },
                    { "anger", 0.05 },
                    { "fear", 0.05 }
                },
                PrimaryEmotion = "joy",
                EmotionalIndicators = new List<string> { "Couleurs vives dominantes" }
            };

            var summary = GenerateSummary(colorAnalysis, shapeAnalysis, emotionalAnalysis);
            var riskLevel = AssessRiskLevel(emotionalAnalysis, shapeAnalysis);

            var analysis = new DrawingAnalysis
            {
                ChildId = childId,
                DrawingUrl = imagePath,
                FileName = Path.GetFileName(imagePath),
                DrawingDate = DateTime.UtcNow,
                Colors = colorAnalysis,
                Shapes = shapeAnalysis,
                Emotions = emotionalAnalysis,
                Summary = summary,
                RiskLevel = riskLevel
            };

            // Sauvegarder en base de données
            await _mongoContext.DrawingAnalyses.InsertOneAsync(analysis);

            _logger.LogInformation("Analyse terminée avec succès pour l'enfant {ChildId}", childId);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse du dessin pour l'enfant {ChildId}", childId);
            throw;
        }
    }

    public async Task<List<DrawingAnalysis>> GetAnalysesByChildIdAsync(string childId)
    {
        // CORRECTION : Utiliser Find avec FilterDefinition
        var filter = Builders<DrawingAnalysis>.Filter.Eq(a => a.ChildId, childId);
        return await _mongoContext.DrawingAnalyses
            .Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<DrawingAnalysis?> GetAnalysisByIdAsync(string analysisId)
    {
        // CORRECTION : Utiliser Find avec FilterDefinition
        var filter = Builders<DrawingAnalysis>.Filter.Eq(a => a.Id, analysisId);
        return await _mongoContext.DrawingAnalyses
            .Find(filter)
            .FirstOrDefaultAsync();
    }

    private string GenerateSummary(ColorAnalysis colors, ShapeAnalysis shapes, EmotionalAnalysis emotions)
    {
        var summaryParts = new List<string>();

        summaryParts.Add($"Émotion primaire : {emotions.PrimaryEmotion}");
        summaryParts.Add($"Couleur dominante : {colors.DominantColor}");

        if (shapes.DetectedShapes.Any())
            summaryParts.Add($"Formes détectées : {string.Join(", ", shapes.DetectedShapes.Take(3))}");

        return string.Join(". ", summaryParts);
    }

    private RiskLevel AssessRiskLevel(EmotionalAnalysis emotions, ShapeAnalysis shapes)
    {
        var riskScore = 0;

        // Logique simplifiée d'évaluation des risques
        if (emotions.EmotionProbabilities.ContainsKey("sadness") &&
            emotions.EmotionProbabilities["sadness"] > 0.3)
            riskScore++;

        if (emotions.EmotionProbabilities.ContainsKey("anger") &&
            emotions.EmotionProbabilities["anger"] > 0.3)
            riskScore += 2;

        return riskScore switch
        {
            0 => RiskLevel.None,
            1 => RiskLevel.Low,
            2 => RiskLevel.Medium,
            3 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }
}
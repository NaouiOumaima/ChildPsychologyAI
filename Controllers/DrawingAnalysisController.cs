using Microsoft.AspNetCore.Mvc;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawingAnalysisController : ControllerBase
{
    private readonly IDrawingAnalysisService _drawingAnalysisService;
    private readonly IAdvancedShapeDetectionService _shapeService;
    private readonly IAdvancedEmotionalAnalysisService _emotionService;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DrawingAnalysisController> _logger;

    public DrawingAnalysisController(
        IDrawingAnalysisService drawingAnalysisService,
        IAdvancedShapeDetectionService shapeService,
        IAdvancedEmotionalAnalysisService emotionService,
        IImageProcessingService imageProcessing,
        IFileStorageService fileStorageService,
        ILogger<DrawingAnalysisController> logger)
    {
        _drawingAnalysisService = drawingAnalysisService;
        _shapeService = shapeService;
        _emotionService = emotionService;
        _imageProcessing = imageProcessing;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<DrawingAnalysis>> AnalyzeDrawing(
        [FromForm] AnalyzeDrawingRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Aucun fichier fourni");

            if (request.File.Length > 10 * 1024 * 1024)
                return BadRequest("Fichier trop volumineux (max 10MB)");

            var filePath = await _fileStorageService.SaveFileAsync(request.File);
            var analysis = await _drawingAnalysisService.AnalyzeDrawingAsync(filePath, request.ChildId);

            _logger.LogInformation("Analyse de dessin complétée pour l'enfant {ChildId}", request.ChildId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse du dessin");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost("analyze-complete")]
    public async Task<ActionResult<CompleteAnalysisResponse>> AnalyzeComplete(
        [FromForm] AnalyzeDrawingRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Aucun fichier fourni");

            var filePath = await _fileStorageService.SaveFileAsync(request.File);
            var image = await _imageProcessing.LoadImageAsync(filePath);

            _logger.LogInformation("Début de l'analyse complète pour l'enfant {ChildId}", request.ChildId);

            // 1. Analyse basique (existante)
            var basicAnalysis = await _drawingAnalysisService.AnalyzeDrawingAsync(filePath, request.ChildId);

            // 2. Analyse avancée des formes
            var advancedShapes = await _shapeService.AnalyzeShapesAsync(image);

            // 3. Analyse avancée des émotions
            var colorService = HttpContext.RequestServices.GetRequiredService<IColorAnalysisService>();
            var colors = await colorService.AnalyzeColorsAsync(image);
            var advancedEmotions = await _emotionService.AnalyzeEmotionsAsync(colors, advancedShapes);

            // 4. Générer la réponse complète
            var completeResponse = new CompleteAnalysisResponse
            {
                BasicAnalysis = basicAnalysis,
                AdvancedShapes = advancedShapes,
                AdvancedEmotions = advancedEmotions,
                FinalSummary = GenerateFinalSummary(basicAnalysis, advancedShapes, advancedEmotions),
                ChildStatus = AssessChildStatus(advancedEmotions, advancedShapes),
                Recommendations = GenerateRecommendations(advancedEmotions, advancedShapes),
                AnalysisTimestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Analyse complète terminée pour l'enfant {ChildId}", request.ChildId);
            return Ok(completeResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse complète");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost("analyze-quick")]
    public async Task<ActionResult<QuickAnalysisResponse>> AnalyzeQuick(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier fourni");

            var filePath = await _fileStorageService.SaveFileAsync(file);
            var image = await _imageProcessing.LoadImageAsync(filePath);

            // Analyses rapides
            var colorService = HttpContext.RequestServices.GetRequiredService<IColorAnalysisService>();
            var colors = await colorService.AnalyzeColorsAsync(image);
            var shapes = await _shapeService.AnalyzeShapesAsync(image);
            var emotions = await _emotionService.AnalyzeEmotionsAsync(colors, shapes);

            var quickResponse = new QuickAnalysisResponse
            {
                DominantColor = colors.DominantColor,
                ColorInterpretations = colors.ColorInterpretations,
                DetectedShapes = shapes.Shapes.Select(s => s.Type).Distinct().ToList(),
                DetectedSymbols = shapes.Symbols.Select(s => s.Type).ToList(),
                DominantEmotion = emotions.DominantEmotion,
                EmotionalState = emotions.EmotionalState,
                RiskLevel = emotions.RiskAssessment.Level,
                QuickSummary = GenerateQuickSummary(colors, shapes, emotions),
                RequiresAttention = emotions.RiskAssessment.RequiresAttention
            };

            return Ok(quickResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse rapide");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("child/{childId}")]
    public async Task<ActionResult<List<DrawingAnalysis>>> GetChildAnalyses(string childId)
    {
        try
        {
            var analyses = await _drawingAnalysisService.GetAnalysesByChildIdAsync(childId);
            return Ok(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des analyses pour l'enfant {ChildId}", childId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("{analysisId}")]
    public async Task<ActionResult<DrawingAnalysis>> GetAnalysis(string analysisId)
    {
        try
        {
            var analysis = await _drawingAnalysisService.GetAnalysisByIdAsync(analysisId);
            if (analysis == null)
                return NotFound();

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'analyse {AnalysisId}", analysisId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost("test-color-analysis")]
    public async Task<ActionResult<ColorAnalysis>> TestColorAnalysis(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier fourni");

            var filePath = await _fileStorageService.SaveFileAsync(file);
            var imageProcessingService = HttpContext.RequestServices.GetRequiredService<IImageProcessingService>();
            var colorAnalysisService = HttpContext.RequestServices.GetRequiredService<IColorAnalysisService>();

            var image = await imageProcessingService.LoadImageAsync(filePath);
            var colorAnalysis = await colorAnalysisService.AnalyzeColorsAsync(image);

            return Ok(colorAnalysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse des couleurs");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost("test-shapes")]
    public async Task<ActionResult<AdvancedShapeAnalysis>> TestShapes(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier fourni");

            var filePath = await _fileStorageService.SaveFileAsync(file);
            var image = await _imageProcessing.LoadImageAsync(filePath);
            var shapes = await _shapeService.AnalyzeShapesAsync(image);

            return Ok(shapes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du test des formes");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    // Méthodes helper
    private string GenerateFinalSummary(DrawingAnalysis basic, AdvancedShapeAnalysis shapes, AdvancedEmotionalAnalysis emotions)
    {
        var summary = new List<string>();

        // Résumé émotionnel
        summary.Add($"Émotion dominante: {emotions.DominantEmotion}");
        summary.Add($"État: {emotions.EmotionalState}");

        // Résumé des couleurs
        if (!string.IsNullOrEmpty(basic.Colors.DominantColor))
        {
            summary.Add($"Couleur: {basic.Colors.DominantColor}");
        }

        // Résumé des formes
        if (shapes.Shapes.Any())
        {
            var mainShapes = shapes.Shapes.OrderByDescending(s => s.Count).Take(2);
            summary.Add($"Formes: {string.Join(", ", mainShapes.Select(s => s.Type))}");
        }

        // Résumé des symboles
        if (shapes.Symbols.Any())
        {
            summary.Add($"Éléments: {string.Join(", ", shapes.Symbols.Select(s => s.Type))}");
        }

        // Évaluation des risques
        summary.Add($"Vigilance: {emotions.RiskAssessment.Level}");

        return string.Join(" | ", summary);
    }

    private string GenerateQuickSummary(ColorAnalysis colors, AdvancedShapeAnalysis shapes, AdvancedEmotionalAnalysis emotions)
    {
        return $"Émotion: {emotions.DominantEmotion} | Couleur: {colors.DominantColor} | Formes: {shapes.Shapes.Count} types | État: {emotions.EmotionalState}";
    }

    private ChildStatusResponse AssessChildStatus(AdvancedEmotionalAnalysis emotions, AdvancedShapeAnalysis shapes)
    {
        var status = new ChildStatusResponse();

        // Évaluation émotionnelle
        if (emotions.DominantEmotion == "joy" && emotions.EmotionalState == "stable")
        {
            status.Mood = "Positif";
            status.Confidence = "Élevée";
        }
        else if (emotions.DominantEmotion == "sadness" || emotions.DominantEmotion == "anxiety")
        {
            status.Mood = "Sensible";
            status.Confidence = emotions.RiskAssessment.Level == "high" ? "Faible" : "Moyenne";
        }
        else
        {
            status.Mood = "Normal";
            status.Confidence = "Normale";
        }

        // Évaluation globale
        status.OverallState = emotions.RiskAssessment.Level switch
        {
            "high" => "Nécessite attention",
            "medium" => "Surveillance",
            _ => "Normal"
        };

        status.IsConcerning = emotions.RiskAssessment.RequiresAttention;

        return status;
    }

    private List<string> GenerateRecommendations(AdvancedEmotionalAnalysis emotions, AdvancedShapeAnalysis shapes)
    {
        var recommendations = new List<string>();

        // Recommandations basées sur l'émotion
        if (emotions.DominantEmotion == "sadness")
        {
            recommendations.Add("Encourager l'expression des émotions");
        }

        if (emotions.DominantEmotion == "anxiety")
        {
            recommendations.Add("Activités relaxantes");
        }

        if (shapes.Composition.SpaceUsage == "constricted")
        {
            recommendations.Add("Encourager l'expression spatiale");
        }

        // Recommandations générales
        if (!recommendations.Any())
        {
            recommendations.Add("Continuer l'observation");
            recommendations.Add("Encourager la créativité");
        }

        return recommendations.Take(3).ToList();
    }
}

// Records pour les requêtes et réponses (ajouter à la fin du fichier)
public record AnalyzeDrawingRequest(IFormFile File, string ChildId);

public class CompleteAnalysisResponse
{
    public DrawingAnalysis BasicAnalysis { get; set; } = new();
    public AdvancedShapeAnalysis AdvancedShapes { get; set; } = new();
    public AdvancedEmotionalAnalysis AdvancedEmotions { get; set; } = new();
    public string FinalSummary { get; set; } = string.Empty;
    public ChildStatusResponse ChildStatus { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
}

public class QuickAnalysisResponse
{
    public string DominantColor { get; set; } = string.Empty;
    public List<string> ColorInterpretations { get; set; } = new();
    public List<string> DetectedShapes { get; set; } = new();
    public List<string> DetectedSymbols { get; set; } = new();
    public string DominantEmotion { get; set; } = string.Empty;
    public string EmotionalState { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string QuickSummary { get; set; } = string.Empty;
    public bool RequiresAttention { get; set; }
}

public class ChildStatusResponse
{
    public string Mood { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public string OverallState { get; set; } = string.Empty;
    public bool IsConcerning { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawingAnalysisController : ControllerBase
{
    private readonly IDrawingAnalysisService _drawingAnalysisService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DrawingAnalysisController> _logger;

    public DrawingAnalysisController(
        IDrawingAnalysisService drawingAnalysisService,
        IFileStorageService fileStorageService,
        ILogger<DrawingAnalysisController> logger)
    {
        _drawingAnalysisService = drawingAnalysisService;
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

            // Vérifier la taille du fichier (max 10MB)
            if (request.File.Length > 10 * 1024 * 1024)
                return BadRequest("Fichier trop volumineux (max 10MB)");

            // Sauvegarder le fichier
            var filePath = await _fileStorageService.SaveFileAsync(request.File);

            // Analyser le dessin
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

            // Utiliser ImageProcessingService directement
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
}

public record AnalyzeDrawingRequest(IFormFile File, string ChildId);
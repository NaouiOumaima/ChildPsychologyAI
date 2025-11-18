using Microsoft.AspNetCore.Mvc;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawingAnalysisController : ControllerBase
{
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IColorAnalysisService _colorAnalysisService;
    private readonly ILogger<DrawingAnalysisController> _logger;

    public DrawingAnalysisController(
        IImageProcessingService imageProcessingService,
        IColorAnalysisService colorAnalysisService,
        ILogger<DrawingAnalysisController> logger)
    {
        _imageProcessingService = imageProcessingService;
        _colorAnalysisService = colorAnalysisService;
        _logger = logger;
    }

    [HttpPost("test-color-analysis")]
    public async Task<ActionResult<ColorAnalysis>> TestColorAnalysis(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier fourni");

            // Sauvegarder temporairement le fichier
            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Charger et analyser l'image
            var image = await _imageProcessingService.LoadImageAsync(tempPath);
            var colorAnalysis = await _colorAnalysisService.AnalyzeColorsAsync(image);

            // Nettoyer le fichier temporaire
            System.IO.File.Delete(tempPath);

            return Ok(colorAnalysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse des couleurs");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("API ChildPsychologyAI fonctionne !");
    }
}
using Microsoft.AspNetCore.Mvc;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;

namespace ChildPsychologyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    private readonly IChildService _childService;
    private readonly ILogger<ChildrenController> _logger;

    public ChildrenController(IChildService childService, ILogger<ChildrenController> logger)
    {
        _childService = childService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Child>> CreateChild([FromBody] CreateChildRequest request)
    {
        try
        {
            var child = new Child
            {
                ParentId = request.ParentId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                ConsentGiven = false
            };

            var createdChild = await _childService.CreateChildAsync(child);
            return Ok(createdChild);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'enfant");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{childId}")]
    public async Task<ActionResult<Child>> GetChild(string childId)
    {
        try
        {
            var child = await _childService.GetChildByIdAsync(childId);
            if (child == null)
                return NotFound();

            return Ok(child);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'enfant {ChildId}", childId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("parent/{parentId}")]
    public async Task<ActionResult<List<Child>>> GetChildrenByParent(string parentId)
    {
        try
        {
            var children = await _childService.GetChildrenByParentIdAsync(parentId);
            return Ok(children);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des enfants du parent {ParentId}", parentId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPut("{childId}/consent")]
    public async Task<ActionResult> GiveConsent(string childId)
    {
        try
        {
            var success = await _childService.GiveConsentAsync(childId);
            if (!success)
                return NotFound();

            return Ok(new { message = "Consentement enregistré avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement du consentement pour l'enfant {ChildId}", childId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPut("{childId}")]
    public async Task<ActionResult<Child>> UpdateChild(string childId, [FromBody] UpdateChildRequest request)
    {
        try
        {
            var existingChild = await _childService.GetChildByIdAsync(childId);
            if (existingChild == null)
                return NotFound();

            existingChild.FirstName = request.FirstName;
            existingChild.LastName = request.LastName;
            existingChild.DateOfBirth = request.DateOfBirth;
            existingChild.Gender = request.Gender;

            var updatedChild = await _childService.UpdateChildAsync(existingChild);
            return Ok(updatedChild);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de l'enfant {ChildId}", childId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}

public record CreateChildRequest(
    string ParentId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender
);

public record UpdateChildRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender
);
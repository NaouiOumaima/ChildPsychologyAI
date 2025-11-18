using Microsoft.AspNetCore.Mvc;
using ChildPsychologyAI.Models.Entities;
using ChildPsychologyAI.Interfaces;
using ChildPsychologyAI.Models.Enums;

namespace ChildPsychologyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> RegisterUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role
                // Note: Dans une vraie application, vous hasherez le mot de passe
            };

            var createdUser = await _userService.CreateUserAsync(user);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription de l'utilisateur");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<User>> GetUser(string userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId}", userId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<User>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par email {Email}", email);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("role/{role}")]
    public async Task<ActionResult<List<User>>> GetUsersByRole(UserRole role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs par rôle {Role}", role);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}

public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string Password // À hasher dans une vraie application
);
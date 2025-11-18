using ChildPsychologyAI.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ChildPsychologyAI.Services.Data;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _storagePath = configuration["FileStorage:LocalPath"] ?? "uploads/drawings";
        _logger = logger;

        // Créer le dossier s'il n'existe pas
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        try
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Fichier sauvegardé : {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier");
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Fichier supprimé : {FilePath}", filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du fichier : {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var fileBytes = File.ReadAllBytes(filePath);
                return Task.FromResult(fileBytes);
            }
            throw new FileNotFoundException($"Fichier non trouvé : {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la lecture du fichier : {FilePath}", filePath);
            throw;
        }
    }
}
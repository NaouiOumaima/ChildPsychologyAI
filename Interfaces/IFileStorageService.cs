using Microsoft.AspNetCore.Http;

namespace ChildPsychologyAI.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
}